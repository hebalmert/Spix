using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppMaui.Services;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;

namespace Spix.AppMaui.ViewModels;

//Fui y no habia nadie. Pide la coordenada ANTES de dejar marcar: es la unica forma de
//saber si de verdad llego hasta la puerta. Si esta lejos avisa, pero lo deja marcar: el
//tecnico no se puede quedar atrapado en la calle, y la oficina lo revisa despues.
[QueryProperty(nameof(VisitId), "id")]
public partial class NoClientViewModel : ObservableObject
{
    private const string BaseUrl = "api/v4/techvisits";

    private readonly IRepository _repository;
    private readonly ApiResponseHandler _responseHandler;
    private readonly LocationService _locationService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private string _visitId = string.Empty;

    [ObservableProperty]
    private TechVisitDto? _visit;

    [ObservableProperty]
    private string _comment = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _coordText = "buscando el GPS...";

    [ObservableProperty]
    private string _distanceText = string.Empty;

    [ObservableProperty]
    private Color _distanceBack = Colors.Transparent;

    [ObservableProperty]
    private Color _distanceForeground = Colors.Black;

    [ObservableProperty]
    private string _warningText = string.Empty;

    //Sin coordenada no hay nada que mandar
    public bool CanMark => _punto is not null;

    private Location? _punto;

    public NoClientViewModel(IRepository repository, ApiResponseHandler responseHandler,
        LocationService locationService, AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _locationService = locationService;
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

        var response = await _repository.GetAsync<TechVisitDto>($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Visit = response.Response;

        await MeasureAsync();
    }

    //Volver a medir: el GPS mejora al rato, y conviene que lo intente antes de marcar
    [RelayCommand]
    private async Task MeasureAsync()
    {
        IsBusy = true;

        try
        {
            _punto = await _locationService.GetAsync();

            if (_punto is null)
            {
                CoordText = "sin GPS";
                DistanceText = string.Empty;
                WarningText = "Sin ubicacion no se puede marcar.";
                OnPropertyChanged(nameof(CanMark));
                return;
            }

            var latitud = VisitDetailViewModel.Redondear(_punto.Latitude);
            var longitud = VisitDetailViewModel.Redondear(_punto.Longitude);

            CoordText = $"{latitud}, {longitud}";

            //La distancia se calcula en el telefono para avisarle en el momento; el
            //servidor la vuelve a calcular al guardar, que es la que vale
            var metros = GeoHelper.Metros(Visit?.ContractLatitude, Visit?.ContractLongitude, latitud, longitud);

            if (metros is null)
            {
                DistanceText = "sin ubicacion en el contrato";
                DistanceBack = Pincel("SpixNoneBack");
                DistanceForeground = Pincel("SpixNoneText");
                WarningText = string.Empty;
            }
            else if (GeoHelper.MismoSitio(metros))
            {
                DistanceText = $"en el sitio - {metros} m";
                DistanceBack = Pincel("SpixOkBack");
                DistanceForeground = Pincel("SpixOkText");
                WarningText = string.Empty;
            }
            else
            {
                DistanceText = $"lejos - {metros} m del sitio";
                DistanceBack = Pincel("SpixFarBack");
                DistanceForeground = Pincel("SpixFarText");
                WarningText = $"Estas a {metros} m de la direccion del contrato. " +
                              "Acercate a la puerta del cliente antes de marcar.";
            }

            OnPropertyChanged(nameof(CanMark));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task MarkAsync()
    {
        if (Visit is null || _punto is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "No estaba el cliente",
            "Queda registrado donde estabas y a que hora.",
            "Marcar");

        if (!confirmado)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var url = $"{BaseUrl}/{Visit.ServiceRequestId}/noclient" +
                      $"?latitude={VisitDetailViewModel.Redondear(_punto.Latitude)}" +
                      $"&longitude={VisitDetailViewModel.Redondear(_punto.Longitude)}" +
                      $"&comment={Uri.EscapeDataString(Comment ?? string.Empty)}";

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

        await _alertService.InfoAsync("Registrado", "La oficina la va a reagendar.");
        await Shell.Current.GoToAsync("//today");
    }

    private static Color Pincel(string clave)
    {
        if (Application.Current?.Resources.TryGetValue(clave, out var valor) == true && valor is Color color)
        {
            return color;
        }

        return Colors.Gray;
    }
}
