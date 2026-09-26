using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.bynode;

// Contratos por AP: de quien cuelga cada antena y cuanto genera ese punto.
//
// Se elige el AP y que estado se quiere mirar, y sale el tablero de lo que factura ese AP
// mas el detalle de sus contratos con el plan que paga cada uno.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik. Por eso las filas
// no tienen ni un boton.
public partial class ReportByNodeIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports";
    private const int PageSize = 20;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportByNodeRow> _rows = new();

    //Las dos listas las arma el backend con su neutro en la posicion 0: aqui no se tocan
    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _nodes = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _states = new();

    [ObservableProperty]
    private Guid _nodeId;

    //Arranca mostrando los activos, que es lo que se mira casi siempre
    [ObservableProperty]
    private int _stateId = (int)ContractState.Active;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    //===== Lo que genera ese AP =====
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _contracts;

    [ObservableProperty]
    private decimal _monthlyTotal;

    [ObservableProperty]
    private int _withoutPlan;

    public string EmptyText => "Elija un AP para ver sus contratos.";

    public ReportByNodeIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Al abrir solo se bajan las dos listas: el reporte espera a que se elija un AP
    public async Task InitializeAsync()
    {
        var estados = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combocontractstates");
        if (!await _responseHandler.HandleErrorAsync(estados))
        {
            States = new ObservableCollection<IntItemModel>(estados.Response ?? new List<IntItemModel>());

            //El combo recien enlazado puede borrar lo elegido si la lista llega despues:
            //se vuelve a fijar el estado con el que arranca la pantalla
            StateId = (int)ContractState.Active;
        }

        var nodos = await _repository.GetAsync<List<GuidItemModel>>($"{BaseUrl}/combonodes");
        if (!await _responseHandler.HandleErrorAsync(nodos))
        {
            Nodes = new ObservableCollection<GuidItemModel>(nodos.Response ?? new List<GuidItemModel>());
        }
    }

    // Cambiar de AP vuelve a preguntar. El neutro de la lista vale Guid vacio y deja la
    // pantalla como al abrirla, sin ningun aviso.
    partial void OnNodeIdChanged(Guid value)
    {
        if (value == Guid.Empty)
        {
            Rows = new ObservableCollection<ReportByNodeRow>();
            HasSummary = false;
            TotalPages = 0;
            CurrentPage = 1;
            return;
        }

        //El aviso de cambio es sincrono: la consulta se suelta y la pantalla no se congela
        _ = RecargarAsync();
    }

    // Cambiar el estado vuelve a preguntar, con el mismo AP elegido
    partial void OnStateIdChanged(int value)
    {
        if (NodeId == Guid.Empty)
        {
            return;
        }

        _ = RecargarAsync();
    }

    private async Task RecargarAsync()
    {
        IsLoading = true;

        try
        {
            CurrentPage = 1;

            await LoadSummaryAsync();
            await LoadAsync(1);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ReportActiveSummaryDto>(
            $"{BaseUrl}/by-node/{NodeId}/summary?stateid={StateId}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new ReportActiveSummaryDto();

        Contracts = datos.Contracts;
        MonthlyTotal = datos.MonthlyTotal;
        WithoutPlan = datos.WithoutPlan;

        HasSummary = true;
    }

    private async Task LoadAsync(int page)
    {
        var response = await _repository.GetAsync<List<ReportActiveContractDto>>(
            $"{BaseUrl}/by-node/{NodeId}?page={page}&recordsnumber={PageSize}&stateid={StateId}");

        //Si falla se deja lo que hubiera en pantalla
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Rows = new ObservableCollection<ReportByNodeRow>(
            (response.Response ?? new List<ReportActiveContractDto>()).Select(x => new ReportByNodeRow(x)));

        CurrentPage = page;

        if (response.HttpResponseMessage is not null &&
            response.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var valores) &&
            int.TryParse(valores.FirstOrDefault(), out var total))
        {
            TotalPages = total;
        }
    }

    // Paginar NO vuelve a pedir el tablero: los numeros son los del AP completo
    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await LoadAsync(page);
    }
}

// Un contrato colgado del AP, ya listo para pintar
public class ReportByNodeRow
{
    public ReportActiveContractDto Item { get; }

    public string ClientFullName => Item.ClientFullName;

    public string ContractText => $"Contrato #{Item.ControlContrato}";

    // Vacio cuando esta activo: asi la pastilla solo se ve cuando hay algo que advertir
    public string? InactiveText => Item.IsActive ? null : "Inactivo";

    public string? ZoneName => Item.ZoneName;

    public string? PlanName => Item.PlanName;

    public decimal PlanPrice => Item.PlanPrice;

    public ReportByNodeRow(ReportActiveContractDto item)
    {
        Item = item;
    }
}
