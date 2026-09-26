using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;

// Una opcion de la lista, ya resuelta: lo que se ve y lo que se guarda.
public class PieceOption
{
    public Guid Value { get; }

    public string Name { get; }

    public PieceOption(Guid value, string? name)
    {
        Value = value;
        Name = name ?? string.Empty;
    }
}

// Cuatro de las piezas del contrato se configuran igual: se elige uno de una lista y se
// guarda. Servidor, IP, Nodo y MAC.
//
// El flujo vive aqui una sola vez; cada pieza solo dice de donde saca su lista, donde se
// guarda y con que nombre viaja el id. Asi no hay cuatro modales casi iguales, pero
// tampoco un modal generico lleno de condicionales.
public abstract partial class ContractPieceDialogViewModel : ObservableObject
{
    protected readonly IRepository _repository;
    protected readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<PieceOption> _options = new();

    [ObservableProperty]
    private Guid _selectedValue;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    protected Guid ContractClientId;

    // De donde sale la lista
    protected abstract Task<List<PieceOption>> CargarOpcionesAsync();

    // Donde se guarda
    protected abstract string SaveUrl { get; }

    // Con que nombre viaja el id elegido
    protected abstract string TargetField { get; }

    // Como se llama la pieza, para los avisos
    protected abstract string PieceName { get; }

    protected ContractPieceDialogViewModel(
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

    public async Task InitializeAsync(Guid contractClientId)
    {
        ContractClientId = contractClientId;

        IsLoading = true;

        try
        {
            Options = new ObservableCollection<PieceOption>(await CargarOpcionesAsync());
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedValue == Guid.Empty)
        {
            await _alertService.WarningAsync(PieceName, $"Debe elegir {PieceName.ToLowerInvariant()}.");
            return;
        }

        IsSaving = true;

        try
        {
            //El contrato y el id elegido: es todo lo que guarda cada una de estas piezas
            var modelo = new Dictionary<string, object>
            {
                ["ContractClientId"] = ContractClientId,
                [TargetField] = SelectedValue
            };

            var response = await _repository.PostAsync(SaveUrl, modelo);
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

    // La lista la arma el backend; aqui solo se proyecta a lo que la pantalla pinta
    protected async Task<List<PieceOption>> PedirAsync<T>(string url, Func<T, PieceOption> proyectar)
    {
        var response = await _repository.GetAsync<List<T>>(url);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return new List<PieceOption>();
        }

        return (response.Response ?? new List<T>()).Select(proyectar).ToList();
    }
}

// El servidor por el que sale el cliente
public partial class ContractServerDialogViewModel : ContractPieceDialogViewModel
{
    public ContractServerDialogViewModel(IRepository r, HttpResponseHandler h, ModalService m, AlertService a)
        : base(r, h, m, a) { }

    protected override string SaveUrl => "api/v1/contractservers";

    protected override string TargetField => "ServerId";

    protected override string PieceName => "El servidor";

    protected override Task<List<PieceOption>> CargarOpcionesAsync()
    {
        return PedirAsync<Server>("api/v1/servers/loadCombo", x => new PieceOption(x.ServerId, x.ServerName));
    }
}

// La IP que se le asigna al cliente
public partial class ContractIpDialogViewModel : ContractPieceDialogViewModel
{
    public ContractIpDialogViewModel(IRepository r, HttpResponseHandler h, ModalService m, AlertService a)
        : base(r, h, m, a) { }

    protected override string SaveUrl => "api/v1/contractips";

    protected override string TargetField => "IpNetId";

    protected override string PieceName => "La IP";

    protected override Task<List<PieceOption>> CargarOpcionesAsync()
    {
        return PedirAsync<IpNet>("api/v1/ipnets/loadCombo", x => new PieceOption(x.IpNetId, x.Ip));
    }
}

// El nodo o AP por el que se conecta
public partial class ContractNodeDialogViewModel : ContractPieceDialogViewModel
{
    public ContractNodeDialogViewModel(IRepository r, HttpResponseHandler h, ModalService m, AlertService a)
        : base(r, h, m, a) { }

    protected override string SaveUrl => "api/v1/contractnodes";

    protected override string TargetField => "NodeId";

    protected override string PieceName => "El nodo";

    protected override Task<List<PieceOption>> CargarOpcionesAsync()
    {
        return PedirAsync<Node>("api/v1/nodes/loadCombo", x => new PieceOption(x.NodeId, x.NodesName));
    }
}

// La MAC del equipo que se le instalo, de los seriales ya cargados
public partial class ContractMacDialogViewModel : ContractPieceDialogViewModel
{
    public ContractMacDialogViewModel(IRepository r, HttpResponseHandler h, ModalService m, AlertService a)
        : base(r, h, m, a) { }

    protected override string SaveUrl => "api/v1/contractmacs";

    protected override string TargetField => "CargueDetailId";

    protected override string PieceName => "El equipo";

    protected override Task<List<PieceOption>> CargarOpcionesAsync()
    {
        return PedirAsync<GuidItemModel>("api/v1/cargueDetails/loadCombo", x => new PieceOption(x.Value, x.Name));
    }
}
