using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesContratos.ContractExempt;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractExempt;

// Exoneracion fija: el contrato mantiene el servicio pero deja de cobrarsele.
//
// No es la exoneracion de un mes —esa es otra pantalla—: aqui no hay vencimiento ni
// prorrateo. El contrato pasa a Exonerado y con eso queda fuera de toda facturacion, que
// solo toma contratos Activos.
//
// No toca el MikroTik en ningun momento: el servicio del cliente no se altera.
public partial class ContractExemptIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractexempt";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<ExemptRow> _rows = new();

    [ObservableProperty]
    private string? _filter;

    [ObservableProperty]
    private DateTime? _desde;

    [ObservableProperty]
    private DateTime? _hasta;

    //Por defecto solo los que siguen exonerados, que es lo que se atiende a diario
    [ObservableProperty]
    private bool _soloAbiertas = true;

    [ObservableProperty]
    private int _openCount;

    [ObservableProperty]
    private decimal _openAmount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private bool _isLoading;

    //El encabezado: cuantos hay exonerados dentro del filtro y cuanto suman
    public string OpenText => $"{OpenCount} exonerados · {OpenAmount.ToString("N2", CultureInfo.CurrentCulture)}";

    public string TotalText => $"Total {TotalAmount.ToString("N2", CultureInfo.CurrentCulture)}";

    public string CountText => $"{TotalCount} registros en el filtro";

    public ContractExemptIndexViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    partial void OnOpenCountChanged(int value) => OnPropertyChanged(nameof(OpenText));

    partial void OnOpenAmountChanged(decimal value) => OnPropertyChanged(nameof(OpenText));

    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(CountText));

    partial void OnTotalAmountChanged(decimal value) => OnPropertyChanged(nameof(TotalText));

    //Los filtros recargan solos: la lista no pagina, viene completa del servidor
    partial void OnDesdeChanged(DateTime? value) => _ = LoadAsync();

    partial void OnHastaChanged(DateTime? value) => _ = LoadAsync();

    partial void OnSoloAbiertasChanged(bool value) => _ = LoadAsync();

    private async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            var url = $"{BaseUrl}/records?soloAbiertas={SoloAbiertas}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter)}";
            }

            if (Desde.HasValue)
            {
                url += $"&desde={Desde.Value:yyyy-MM-dd}";
            }

            if (Hasta.HasValue)
            {
                url += $"&hasta={Hasta.Value:yyyy-MM-dd}";
            }

            var response = await _repository.GetAsync<ExemptListDTO>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            var datos = response.Response ?? new ExemptListDTO();

            Rows = new ObservableCollection<ExemptRow>(datos.Records.Select(x => new ExemptRow(x)));
            OpenCount = datos.OpenCount;
            OpenAmount = datos.OpenAmount;
            TotalCount = datos.TotalCount;
            TotalAmount = datos.TotalAmount;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<ContractExemptDialogView>("Exonerar contrato");

        if (result.Succeeded)
        {
            await LoadAsync();
            await _alertService.SuccessAsync("Exonerar contrato", "El contrato quedo exonerado.");
        }
    }

    //Quien la registro no ocupa una columna: se consulta aqui
    [RelayCommand]
    private async Task AuditAsync(ExemptRow? item)
    {
        if (item is null)
        {
            return;
        }

        var lineas = new List<string>
        {
            $"Registrado por: {item.UserByName}",
            $"Fecha: {item.DateExempt.ToLocalTime():dd/MM/yyyy HH:mm}"
        };

        if (!string.IsNullOrWhiteSpace(item.Motivo))
        {
            lineas.Add($"Motivo: {item.Motivo}");
        }

        if (item.DateEnded.HasValue)
        {
            lineas.Add($"Retirada por: {item.UserByNameEnded}");
            lineas.Add($"Fecha de retiro: {item.DateEnded.Value.ToLocalTime():dd/MM/yyyy HH:mm}");
        }

        await _alertService.WarningAsync("Auditoria del registro", string.Join(Environment.NewLine, lineas));
    }

    [RelayCommand]
    private async Task ReasonAsync(ExemptRow? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.Motivo))
        {
            return;
        }

        await _alertService.WarningAsync("Motivo", item.Motivo);
    }

    // Retirar la exoneracion: el contrato vuelve a Activo y se le empieza a cobrar de nuevo
    [RelayCommand]
    private async Task ActivateAsync(ExemptRow? item)
    {
        if (item is null || item.DateEnded.HasValue)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Retirar la exoneracion",
            $"El contrato {item.ControlContrato} de {item.ClientName} volvera a Activo y se le empezara a cobrar de nuevo. Desea continuar?",
            "Retirar la exoneracion");

        if (!confirmado)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var response = await _repository.PostAsync($"{BaseUrl}/{item.ContractClientId}/activate", new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await LoadAsync();
            await _alertService.SuccessAsync("Retirar la exoneracion", "El contrato volvio a Activo.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

// Una fila de la tabla, ya lista para pintar
public class ExemptRow
{
    public ExemptRecordDTO Item { get; }

    public Guid ContractClientId => Item.ContractClientId;

    public long ControlContrato => Item.ControlContrato;

    public string? ClientName => Item.ClientName;

    public string? ClientDocument => Item.ClientDocument;

    public string? ContractAddress => Item.ContractAddress;

    public string Place => string.Join(" · ",
        new[] { Item.CityName, Item.ZoneName }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public string? PlanName => Item.PlanName;

    public decimal PlanAmount => Item.PlanAmount;

    public string DateExemptText => Item.DateExempt.ToLocalTime().ToString("dd/MM/yyyy");

    // Mientras no tenga fecha de retiro, el contrato sigue sin cobrarse
    public bool IsOpen => Item.DateEnded is null;

    public bool IsClosed => Item.DateEnded is not null;

    public bool HasReason => !string.IsNullOrWhiteSpace(Item.Motivo);

    public string? Motivo => Item.Motivo;

    public string? UserByName => Item.UserByName;

    public string? UserByNameEnded => Item.UserByNameEnded;

    public DateTime DateExempt => Item.DateExempt;

    public DateTime? DateEnded => Item.DateEnded;

    public ExemptRow(ExemptRecordDTO item)
    {
        Item = item;
    }
}

// Exonerar un contrato: se busca entre los ACTIVOS, se elige y se explica por que.
public partial class ContractExemptDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractexempt";

    //Minimo de letras y pausa antes de consultar, igual que el autocompletar de la web
    private const int MinimoParaBuscar = 3;
    private const int PausaMs = 500;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private ObservableCollection<ActiveContractDTO> _candidates = new();

    [ObservableProperty]
    private ActiveContractDTO? _selected;

    [ObservableProperty]
    private string? _motivo;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _noResults;

    private CancellationTokenSource? _cts;

    public bool HasSelected => Selected is not null;

    public string SelectedText => Selected is null
        ? string.Empty
        : $"#{Selected.ControlContrato} · {Selected.ClientName}   ({Selected.PlanName} · {Selected.PlanAmount:N2})";

    public ContractExemptDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
    }

    partial void OnSelectedChanged(ActiveContractDTO? value)
    {
        OnPropertyChanged(nameof(HasSelected));
        OnPropertyChanged(nameof(SelectedText));
    }

    // Solo se ofrecen contratos ACTIVOS. Admite varias busquedas a la vez para no perder
    // teclas, y espera medio segundo antes de consultar.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SearchAsync(string? texto)
    {
        SearchText = texto;
        Selected = null;

        _cts?.Cancel();

        if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < MinimoParaBuscar)
        {
            Candidates = new ObservableCollection<ActiveContractDTO>();
            IsSearching = false;
            NoResults = false;
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await Task.Delay(PausaMs, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        IsSearching = true;
        NoResults = false;

        var response = await _repository.GetAsync<List<ActiveContractDTO>>(
            $"{BaseUrl}/active?filter={Uri.EscapeDataString(texto.Trim())}");

        if (token.IsCancellationRequested)
        {
            return;
        }

        IsSearching = false;

        if (await _responseHandler.HandleErrorAsync(response))
        {
            Candidates = new ObservableCollection<ActiveContractDTO>();
            return;
        }

        Candidates = new ObservableCollection<ActiveContractDTO>(response.Response ?? new List<ActiveContractDTO>());

        NoResults = Candidates.Count == 0;
    }

    [RelayCommand]
    private void Select(ActiveContractDTO? item)
    {
        if (item is null)
        {
            return;
        }

        Selected = item;

        _cts?.Cancel();
        IsSearching = false;
        NoResults = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Selected is null)
        {
            await _alertService.WarningAsync("Exonerar contrato", "Debe elegir el contrato.");
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Exonerar contrato",
            $"Se exonerara el contrato {Selected.ControlContrato} de {Selected.ClientName}: el servicio no se toca, solo deja de cobrarsele. Desea continuar?",
            "Exonerar");

        if (!confirmado)
        {
            return;
        }

        IsSaving = true;

        try
        {
            var url = $"{BaseUrl}/{Selected.ContractClientId}/exempt" +
                      $"?motivo={Uri.EscapeDataString(Motivo ?? string.Empty)}";

            var response = await _repository.PostAsync(url, new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
