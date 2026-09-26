using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesContratos.ContractClient;
using Spix.AppWpf.Views.EntitiesContratos.ContractControl;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ContractClientEntity = Spix.Domain.EntitiesContratos.ContractClient;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;

// Control de contratos: el seguimiento de los contratos YA OPERATIVOS.
//
// Es la pantalla hermana de Contratos, pero con otro publico: alla se arma el expediente
// (documentos, fotos, firmas) y aqui se administra el servicio. Por eso el filtro solo
// ofrece los estados operativos —En proceso, Activo, Exonerado, Suspendido— y no Borrador
// ni Pendiente de aprobacion.
//
// El trabajo de verdad esta en el DETALLE de cada contrato: alli se configura el servicio
// y se cambia el estado, que es lo unico que toca el MikroTik.
public partial class ContractControlIndexViewModel : PagedListViewModel<ContractClientEntity>
{
    private const string BaseUrl = "api/v1/contractcontrols";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;
    private readonly LanguageService _languageService;
    private readonly NavigationService _navigationService;

    protected override string Endpoint => StatusFilter > 0
        ? $"{BaseUrl}?id={StatusFilter}"
        : BaseUrl;

    [ObservableProperty]
    private ObservableCollection<ContractClientRow> _rows = new();

    [ObservableProperty]
    private ObservableCollection<ContractStateOption> _statuses = new();

    private int _statusFilter;

    public int StatusFilter
    {
        get => _statusFilter;
        set
        {
            if (_statusFilter == value)
            {
                return;
            }

            _statusFilter = value;
            OnPropertyChanged();

            _ = LoadAsync(1);
        }
    }

    private readonly Dictionary<int, string> _nombresDeEstado = new();

    public ContractControlIndexViewModel(
        IPagedEntityService<ContractClientEntity> pagedEntityService,
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler,
        LanguageService languageService,
        NavigationService navigationService)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
        _languageService = languageService;
        _navigationService = navigationService;

        ArmarEstados();
    }

    protected override Task AfterLoadAsync()
    {
        Rows = new ObservableCollection<ContractClientRow>(
            Items.Select(x => new ContractClientRow(x, _nombresDeEstado)));

        return Task.CompletedTask;
    }

    // El detalle es una PANTALLA: ahi vive toda la configuracion del servicio
    [RelayCommand]
    private void OpenDetail(ContractClientRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        _navigationService.Show<ContractControlDetailView>(
            "Control de contratos",
            $"Operaciones / Contrato {fila.Number}",
            vista =>
            {
                vista.Prepare(fila.Item.ContractClientId);

                vista.BackRequested += (_, _) => VolverAlListado();
            });
    }

    private void VolverAlListado()
    {
        _navigationService.Show<Views.EntitiesContratos.ContractControl.ContractControlIndexView>(
            "Control de contratos",
            "Operaciones / Control de contratos");
    }

    [RelayCommand]
    private async Task AuditAsync(ContractClientRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = fila.Item.ContractClientId
        };

        await _modalService.ShowAsync<ContractAuditDialogView>($"Contrato {fila.Number}", parametros);
    }

    [RelayCommand]
    private async Task EditAsync(ContractClientRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.Item.ContractClientId
        };

        var result = await _modalService.ShowAsync<EditContractClientDialogView>(
            $"Editar contrato {fila.Number}", parametros);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El contrato fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateContractClientDialogView>("Crear contrato");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El contrato fue creado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ContractClientRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Eliminar contrato",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{BaseUrl}/{fila.Item.ContractClientId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El contrato fue eliminado correctamente.");
    }

    // Esta pantalla solo maneja contratos ya operativos: por eso no aparecen Borrador,
    // Pendiente de aprobacion, Anulado ni Retirado.
    private void ArmarEstados()
    {
        var opciones = new List<ContractStateOption>
        {
            new(0, _languageService.Text("Filter_AllStatus")),
            new((int)ContractState.InProgress, Nombre(ContractState.InProgress)),
            new((int)ContractState.Active, Nombre(ContractState.Active)),
            new((int)ContractState.Exempt, Nombre(ContractState.Exempt)),
            new((int)ContractState.Suspended, Nombre(ContractState.Suspended))
        };

        Statuses = new ObservableCollection<ContractStateOption>(opciones);

        foreach (ContractState estado in Enum.GetValues<ContractState>())
        {
            _nombresDeEstado[(int)estado] = Nombre(estado);
        }
    }

    private string Nombre(ContractState estado)
    {
        return _languageService.Text($"ContractState_{estado}");
    }
}
