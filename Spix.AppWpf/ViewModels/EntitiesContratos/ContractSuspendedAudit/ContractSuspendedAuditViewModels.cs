using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesContratos.ContractControl;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractSuspendedAudit;

// Auditoria de contratos activados: quien le devolvio el servicio a quien y cuando.
//
// Es de SOLA CONSULTA: no escribe nada y no toca el MikroTik. Lee la bitacora del contrato,
// filtrada por el evento de reactivacion y por un rango de fechas.
//
// Ojo con lo que NO sale aqui: la reactivacion masiva no deja evento de bitacora, solo
// cierra la suspension. Asi es en la web y asi se dejo en el escritorio, para que las dos
// muestren lo mismo.
public partial class ContractSuspendedAuditIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractsuspendedaudits";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<ContractSuspendedAuditRow> _rows = new();

    //La ultima semana, que es lo que se consulta a diario
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private bool _isLoading;

    public string CountText => $"{Rows.Count} reactivaciones en el rango";

    public ContractSuspendedAuditIndexViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        NavigationService navigationService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _navigationService = navigationService;
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    //Cambiar cualquiera de las dos fechas vuelve a consultar, igual que en la web
    partial void OnStartDateChanged(DateTime value) => _ = LoadAsync();

    partial void OnEndDateChanged(DateTime value) => _ = LoadAsync();

    private async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<ContractSuspendedAuditDTO>>(
                $"{BaseUrl}?startDate={StartDate:yyyy-MM-dd}&endDate={EndDate:yyyy-MM-dd}");

            //Si falla se deja lo que habia: no se vacia la tabla por un error de red
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<ContractSuspendedAuditRow>(
                (response.Response ?? new List<ContractSuspendedAuditDTO>())
                    .Select(x => new ContractSuspendedAuditRow(x)));

            OnPropertyChanged(nameof(CountText));
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

    // Ir al contrato: el detalle es una PANTALLA, y desde alli se vuelve aqui
    [RelayCommand]
    private void OpenContract(ContractSuspendedAuditRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        _navigationService.Show<ContractControlDetailView>(
            "Control de contratos",
            $"Operaciones / Contrato {fila.ControlContrato}",
            vista =>
            {
                vista.Prepare(fila.ContractId);

                vista.BackRequested += (_, _) => VolverALaAuditoria();
            });
    }

    private void VolverALaAuditoria()
    {
        _navigationService.Show<Views.EntitiesContratos.ContractSuspendedAudit.ContractSuspendedAuditIndexView>(
            "Auditoria de activaciones",
            "Operaciones / Auditoria de activaciones");
    }
}

// Una fila de la auditoria, ya lista para pintar
public class ContractSuspendedAuditRow
{
    public ContractSuspendedAuditDTO Item { get; }

    // Es el ContractClientId: con el se abre el detalle del contrato
    public Guid ContractId => Item.ContractId;

    public long ControlContrato => Item.ControlContrato;

    public string ClientDocument => Item.ClientDocument;

    public string ClientFullName => Item.ClientFullName;

    public string UserByName => Item.UserByName;

    public string DateModifiedText => Item.DateModified.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt");

    public ContractSuspendedAuditRow(ContractSuspendedAuditDTO item)
    {
        Item = item;
    }
}
