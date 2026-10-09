using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppMaui.Services;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;

namespace Spix.AppMaui.ViewModels;

//La visita abierta. Es la pantalla donde el tecnico trabaja: marca que llego, escribe
//lo que hizo y cierra, o dice que no habia nadie.
[QueryProperty(nameof(VisitId), "id")]
public partial class VisitDetailViewModel : ObservableObject
{
    private const string BaseUrl = "api/v4/techvisits";

    private readonly IRepository _repository;
    private readonly ApiResponseHandler _responseHandler;
    private readonly LocationService _locationService;
    private readonly PhotoService _photoService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private string _visitId = string.Empty;

    [ObservableProperty]
    private TechVisitDto? _visit;

    [ObservableProperty]
    private string _comment = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    //Lo que la pantalla necesita saber sin volver a preguntar
    public bool IsPending => Visit is not null && (ScheduleStatus)Visit.Status == ScheduleStatus.Pending;

    public bool IsInProgress => Visit is not null && (ScheduleStatus)Visit.Status == ScheduleStatus.InProgress;

    public bool HasLocation => Visit?.Latitude is not null;

    public string DistanceText { get; private set; } = string.Empty;

    public Color DistanceBack { get; private set; } = Colors.Transparent;

    public Color DistanceText2 { get; private set; } = Colors.Black;

    public bool HasService => Visit?.HasService == true;

    public bool HasPhoto => Visit?.HasAfterPhoto == true;

    public string ServiceText => HasService ? "Servicio cargado" : "Falta el servicio";

    public string PhotoText => HasPhoto ? "Foto cargada" : "Falta la foto";

    public string CoordText => Visit?.Latitude is null
        ? "sin marcar"
        : $"{Visit.Latitude}, {Visit.Longitude}";

    public VisitDetailViewModel(IRepository repository, ApiResponseHandler responseHandler,
        LocationService locationService, PhotoService photoService, AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _locationService = locationService;
        _photoService = photoService;
        _alertService = alertService;
    }

    partial void OnVisitIdChanged(string value) => _ = LoadAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (!Guid.TryParse(VisitId, out var id))
        {
            return;
        }

        IsBusy = true;

        try
        {
            var response = await _repository.GetAsync<TechVisitDto>($"{BaseUrl}/{id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Visit = response.Response;
            Comment = Visit?.TechnicianComment ?? string.Empty;

            Refrescar();
        }
        finally
        {
            IsBusy = false;
        }
    }

    //Llegue: la visita pasa a En sitio y de una vez se manda donde estoy
    [RelayCommand]
    private async Task StartAsync()
    {
        if (Visit is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var response = await _repository.PostAsync<object, TechVisitDto>(
                $"{BaseUrl}/{Visit.ServiceRequestId}/start", new { });

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Visit = response.Response;
            Refrescar();
        }
        finally
        {
            IsBusy = false;
        }

        await CaptureAsync();
    }

    //Donde estoy. Se puede repetir: si el GPS mejora, la ultima manda.
    [RelayCommand]
    private async Task CaptureAsync()
    {
        if (Visit is null)
        {
            return;
        }

        var punto = await _locationService.GetAsync();
        if (punto is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var url = $"{BaseUrl}/{Visit.ServiceRequestId}/capturelocation" +
                      $"?latitude={Redondear(punto.Latitude)}&longitude={Redondear(punto.Longitude)}";

            var response = await _repository.PostAsync<object, ServiceRequestDto>(url, new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }
        }
        finally
        {
            IsBusy = false;
        }

        await LoadAsync();
    }

    //La foto del trabajo terminado: es una de las tres condiciones del cierre.
    //No queda nada en el telefono, se manda y se acaba.
    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        if (Visit is null)
        {
            return;
        }

        var base64 = await _photoService.TakeBase64Async();
        if (string.IsNullOrWhiteSpace(base64))
        {
            return;
        }

        IsBusy = true;

        try
        {
            var dto = new ServiceRequestPhotoDto
            {
                ServiceRequestId = Visit.ServiceRequestId,
                PhotoType = ServicePhotoType.After,
                ImgBase64 = base64
            };

            var response = await _repository.PostAsync<ServiceRequestPhotoDto, ServiceRequestPhotoDto>(
                "api/v4/techvisits/photo", dto);

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }
        }
        finally
        {
            IsBusy = false;
        }

        await LoadAsync();
    }

    //El servicio realizado va en su propia pantalla: son dos listas y un precio
    [RelayCommand]
    private async Task AddServiceAsync()
    {
        if (Visit is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"addservice?id={Visit.ServiceRequestId}");
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        if (Visit is null)
        {
            return;
        }

        //Las tres condiciones las decide el servidor; aqui solo se explica que falta
        if (!Visit.CanClose && string.IsNullOrWhiteSpace(Comment))
        {
            await _alertService.WarningAsync("Cerrar visita",
                "Falta el comentario de lo que hiciste.");
            return;
        }

        if (!Visit.HasService)
        {
            await _alertService.WarningAsync("Cerrar visita", "Falta cargar el servicio realizado.");
            return;
        }

        if (!Visit.HasAfterPhoto)
        {
            await _alertService.WarningAsync("Cerrar visita", "Falta la foto del trabajo terminado.");
            return;
        }

        IsBusy = true;

        try
        {
            var url = $"{BaseUrl}/{Visit.ServiceRequestId}/close" +
                      $"?comment={Uri.EscapeDataString(Comment ?? string.Empty)}";

            var response = await _repository.PostAsync<object, ServiceRequestDto>(url, new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }
        }
        finally
        {
            IsBusy = false;
        }

        await _alertService.InfoAsync("Visita cerrada", "Quedo registrada.");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task NoClientAsync()
    {
        if (Visit is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"noclient?id={Visit.ServiceRequestId}");
    }

    private void Refrescar()
    {
        var metros = Visit?.DistanceMeters;

        if (metros is null)
        {
            DistanceText = Visit?.Latitude is null ? string.Empty : "sin comparar";
            DistanceBack = Pincel("SpixNoneBack");
            DistanceText2 = Pincel("SpixNoneText");
        }
        else if (GeoHelper.MismoSitio(metros))
        {
            DistanceText = $"en el sitio - {metros} m";
            DistanceBack = Pincel("SpixOkBack");
            DistanceText2 = Pincel("SpixOkText");
        }
        else
        {
            DistanceText = $"lejos - {metros} m";
            DistanceBack = Pincel("SpixFarBack");
            DistanceText2 = Pincel("SpixFarText");
        }

        OnPropertyChanged(nameof(IsPending));
        OnPropertyChanged(nameof(IsInProgress));
        OnPropertyChanged(nameof(HasLocation));
        OnPropertyChanged(nameof(HasService));
        OnPropertyChanged(nameof(HasPhoto));
        OnPropertyChanged(nameof(ServiceText));
        OnPropertyChanged(nameof(PhotoText));
        OnPropertyChanged(nameof(CoordText));
        OnPropertyChanged(nameof(DistanceText));
        OnPropertyChanged(nameof(DistanceBack));
        OnPropertyChanged(nameof(DistanceText2));
    }

    //Siete decimales, los mismos que guarda la base
    internal static decimal Redondear(double valor) => Math.Round((decimal)valor, 7);

    private static Color Pincel(string clave)
    {
        if (Application.Current?.Resources.TryGetValue(clave, out var valor) == true && valor is Color color)
        {
            return color;
        }

        return Colors.Gray;
    }
}
