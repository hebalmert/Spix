using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;

// La MAC del equipo del cliente, en cascada: Categoria equipo -> Equipo -> MAC.
//
// Es el gemelo de FormContractMac del Blazor. Tiene ViewModel propio y no hereda del de las
// otras piezas (Servidor, IP, Nodo) porque aquellas eligen UNA lista y esta encadena tres:
// meterlo en el generico lo llenaria de condicionales. Es la misma razon por la que en el
// Blazor tambien esta aparte.
//
// Las tres listas salen armadas del backend y solo traen lo que tiene seriales disponibles,
// asi que no aparece una categoria ni un equipo sin MAC libres.
public partial class ContractMacDialogViewModel : ObservableObject
{
    private const string SaveUrl = "api/v1/contractmacs";
    private const string CombosUrl = "api/v1/cargueDetails";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    private Guid _contractClientId;

    [ObservableProperty] private ObservableCollection<PieceOption> _categories = new();
    [ObservableProperty] private ObservableCollection<PieceOption> _products = new();
    [ObservableProperty] private ObservableCollection<PieceOption> _macs = new();

    [ObservableProperty] private Guid _selectedCategoryId;
    [ObservableProperty] private Guid _selectedProductId;
    [ObservableProperty] private Guid _selectedMacId;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;

    //Hasta que no se elige el escalon de arriba, el de abajo no se puede tocar
    public bool CanChooseProduct => SelectedCategoryId != Guid.Empty;

    public bool CanChooseMac => SelectedProductId != Guid.Empty;

    public ContractMacDialogViewModel(IRepository repository, HttpResponseHandler responseHandler,
        ModalService modalService, AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
    }

    public async Task InitializeAsync(Guid contractClientId)
    {
        _contractClientId = contractClientId;

        IsLoading = true;
        try
        {
            Categories = new ObservableCollection<PieceOption>(
                await PedirAsync($"{CombosUrl}/comboCategories"));
        }
        finally
        {
            IsLoading = false;
        }
    }

    //Al cambiar de categoria se olvidan el equipo y la MAC: eran de la categoria anterior
    public async Task ChangeCategoryAsync(Guid productCategoryId)
    {
        SelectedCategoryId = productCategoryId;
        SelectedProductId = Guid.Empty;
        SelectedMacId = Guid.Empty;
        Products = new ObservableCollection<PieceOption>();
        Macs = new ObservableCollection<PieceOption>();
        Avisar();

        if (productCategoryId == Guid.Empty)
        {
            return;
        }

        IsLoading = true;
        try
        {
            Products = new ObservableCollection<PieceOption>(
                await PedirAsync($"{CombosUrl}/comboProducts/{productCategoryId}"));
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ChangeProductAsync(Guid productId)
    {
        SelectedProductId = productId;
        SelectedMacId = Guid.Empty;
        Macs = new ObservableCollection<PieceOption>();
        Avisar();

        if (productId == Guid.Empty)
        {
            return;
        }

        IsLoading = true;
        try
        {
            Macs = new ObservableCollection<PieceOption>(
                await PedirAsync($"{CombosUrl}/comboMacs/{productId}"));
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ChangeMac(Guid cargueDetailId)
    {
        SelectedMacId = cargueDetailId;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedMacId == Guid.Empty)
        {
            await _alertService.WarningAsync("El equipo", "Debe elegir la MAC del equipo.");
            return;
        }

        IsSaving = true;
        try
        {
            //Del contrato solo se guarda la MAC: la categoria y el equipo son el camino para
            //llegar a ella, igual que en el Blazor.
            var modelo = new Dictionary<string, object>
            {
                ["ContractClientId"] = _contractClientId,
                ["CargueDetailId"] = SelectedMacId
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

    private void Avisar()
    {
        OnPropertyChanged(nameof(CanChooseProduct));
        OnPropertyChanged(nameof(CanChooseMac));
    }

    // La lista la arma el backend; aqui solo se proyecta a lo que la pantalla pinta
    private async Task<List<PieceOption>> PedirAsync(string url)
    {
        var response = await _repository.GetAsync<List<GuidItemModel>>(url);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return new List<PieceOption>();
        }

        return (response.Response ?? new List<GuidItemModel>())
            .Select(x => new PieceOption(x.Value, x.Name))
            .ToList();
    }
}
