using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesSchedule.VisitReview;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Spix.AppWpf.ViewModels.EntitiesSchedule.VisitReview;

// Una fila de la bandeja, YA LISTA PARA PINTAR: la pantalla no calcula distancias ni
// colores ni que botones mostrar. Misma idea que ServiceRequestRow.
public class VisitReviewRow
{
    public VisitReviewDto Item { get; }

    public long Number => Item.RequestNumber;

    public string ClientName => Item.ClientFullName;

    public string ClientMeta => $"Contrato {Item.ControlContrato}";

    public string VisitMeta { get; }

    public string? Comment => Item.TechnicianComment;

    public string ContractCoord { get; }

    public string VisitCoord { get; }

    public string DistanceText { get; }

    public Brush DistanceBack { get; }

    public Brush DistanceForeground { get; }

    //Marco lejos del sitio: se ve de un golpe cual hay que revisar
    public bool IsFar => !Item.SameSite && Item.DistanceMeters is not null;

    //La ubicacion se aplica o se descarta; la visita sin cliente se reagenda
    public bool CanApply => !Item.ClientAbsent;

    public bool CanReschedule => Item.ClientAbsent && !Item.AlreadyRescheduled;

    public VisitReviewRow(VisitReviewDto item)
    {
        Item = item;

        var cuando = item.CompletedAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "-";

        VisitMeta = item.ClientAbsent
            ? $"Visita #{item.RequestNumber} · {item.OriginName} · {item.TechnicianName} · {cuando} · intento {item.Attempt}"
            : $"Visita #{item.RequestNumber} · {item.OriginName} · {item.TechnicianName} · {cuando}";

        ContractCoord = Coordenada(item.ContractLatitude, item.ContractLongitude);
        VisitCoord = Coordenada(item.VisitLatitude, item.VisitLongitude);

        DistanceText = TextoDeDistancia(item);

        var tono = item.DistanceMeters is null
            ? "None"
            : item.SameSite ? "Ok" : "Far";

        DistanceBack = Pincel($"BrushReviewDist{tono}Back");
        DistanceForeground = Pincel($"BrushReviewDist{tono}Text");
    }

    private static string Coordenada(decimal? latitud, decimal? longitud)
    {
        if (latitud is null || longitud is null)
        {
            return "sin ubicacion";
        }

        return $"{latitud}, {longitud}";
    }

    private static string TextoDeDistancia(VisitReviewDto item)
    {
        if (item.DistanceMeters is null)
        {
            return "sin comparar";
        }

        return item.SameSite
            ? $"en el sitio · {item.DistanceMeters} m"
            : $"lejos · {item.DistanceMeters} m";
    }

    private static Brush Pincel(string clave)
    {
        return Application.Current.TryFindResource(clave) as Brush ?? Brushes.Gray;
    }
}

// La bandeja de revision. Replicado de /visitreviews de la web: dos pestañas, dos
// endpoints, el front no filtra nada.
public partial class VisitReviewIndexViewModel : PagedListViewModel<VisitReviewDto>
{
    private const string BaseUrl = "api/v1/visitreviews";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    //Cada pestaña es su propio endpoint
    protected override string Endpoint => $"{BaseUrl}/{(Tab == 0 ? "location" : "absent")}";

    [ObservableProperty]
    private ObservableCollection<VisitReviewRow> _rows = new();

    [ObservableProperty]
    private VisitReviewCountersDto? _counters;

    //0 = Ubicacion, 1 = Sin cliente
    [ObservableProperty]
    private int _tab;

    public bool IsLocationTab => Tab == 0;

    public bool IsAbsentTab => Tab == 1;

    public VisitReviewIndexViewModel(
        IPagedEntityService<VisitReviewDto> pagedEntityService,
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    public async Task InitializeAsync()
    {
        await LoadCountersAsync();
        await LoadAsync();
    }

    protected override Task AfterLoadAsync()
    {
        Rows = new ObservableCollection<VisitReviewRow>(Items.Select(x => new VisitReviewRow(x)));

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ChangeTabAsync(string? valor)
    {
        if (!int.TryParse(valor, out var pestania) || pestania == Tab)
        {
            return;
        }

        Tab = pestania;

        await LoadAsync(1);
    }

    partial void OnTabChanged(int value)
    {
        OnPropertyChanged(nameof(IsLocationTab));
        OnPropertyChanged(nameof(IsAbsentTab));
    }

    // Aplicar escribe la coordenada del tecnico en el contrato, sin transcribir nada a mano
    [RelayCommand]
    private async Task ApplyAsync(VisitReviewRow? fila)
    {
        if (fila is null || !fila.CanApply)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Aplicar ubicacion",
            $"La ubicacion del contrato {fila.Item.ControlContrato} queda en la que tomo el tecnico.",
            "Aplicar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.PostAsync<object, bool>(
            $"{BaseUrl}/{fila.Item.ServiceRequestId}/applylocation", new { });

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await RecargarAsync();

        await _alertService.SuccessAsync("Revision", "Ubicacion aplicada al contrato.");
    }

    // Descartar deja el contrato como esta y saca la visita de la bandeja
    [RelayCommand]
    private async Task DismissAsync(VisitReviewRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Dejar como esta",
            "La ubicacion del contrato no cambia y la visita sale de la bandeja.",
            "Descartar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.PostAsync<object, bool>(
            $"{BaseUrl}/{fila.Item.ServiceRequestId}/dismiss", new { });

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await RecargarAsync();
    }

    [RelayCommand]
    private async Task RescheduleAsync(VisitReviewRow? fila)
    {
        if (fila is null || !fila.CanReschedule)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            { "ServiceRequestId", fila.Item.ServiceRequestId },
            { "RequestNumber", fila.Item.RequestNumber },
            { "ClientFullName", fila.Item.ClientFullName }
        };

        var result = await _modalService.ShowAsync<RescheduleVisitDialogView>("Reagendar", parametros);
        if (!result.Succeeded)
        {
            return;
        }

        await RecargarAsync();

        await _alertService.SuccessAsync("Revision", "Visita reagendada.");
    }

    // El rastro de la visita: donde estuvo el tecnico y a que hora
    [RelayCommand]
    private async Task AuditAsync(VisitReviewRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var item = fila.Item;

        var lineas = new List<string>
        {
            $"Visita: #{item.RequestNumber}",
            $"Tecnico: {(string.IsNullOrWhiteSpace(item.TechnicianName) ? "-" : item.TechnicianName)}",
            $"Cerrada: {(item.CompletedAtUtc is null ? "-" : item.CompletedAtUtc.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm"))}",
            $"Contrato: {fila.ContractCoord}",
            $"Tecnico marco: {fila.VisitCoord}",
            $"Diferencia: {fila.DistanceText}"
        };

        await _alertService.SuccessAsync($"Contrato {item.ControlContrato}", string.Join(Environment.NewLine, lineas));
    }

    private async Task RecargarAsync()
    {
        await LoadCountersAsync();
        await LoadAsync(CurrentPage);
    }

    private async Task LoadCountersAsync()
    {
        var response = await _repository.GetAsync<VisitReviewCountersDto>($"{BaseUrl}/counters");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Counters = response.Response;
    }
}
