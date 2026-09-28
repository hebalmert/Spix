using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractMacPage;

public partial class FormContractMac
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public ContractMac ContractMac { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private List<GuidItemModel>? Categories = new();
    private List<GuidItemModel>? Products = new();
    private List<GuidItemModel>? ListMacs = new();

    //Los dos primeros escalones solo sirven para llegar a la MAC: no se guardan en el contrato
    private Guid SelectedCategoryId;
    private Guid SelectedProductId;

    private string BaseView = "/contractcontrol";
    private string BaseComboCategories = "/api/v1/cargueDetails/comboCategories";
    private string BaseComboProducts = "/api/v1/cargueDetails/comboProducts";
    private string BaseComboMacs = "/api/v1/cargueDetails/comboMacs";

    protected override async Task OnInitializedAsync()
    {
        await LoadCategories();
    }

    private async Task LoadCategories()
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>($"{BaseComboCategories}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        Categories = responseHttp.Response;
    }

    //Al cambiar de categoria se olvidan el equipo y la MAC: eran de la categoria anterior
    private async Task CategoryChanged(ChangeEventArgs e)
    {
        SelectedCategoryId = Guid.TryParse(e.Value?.ToString(), out var categoryId) ? categoryId : Guid.Empty;
        SelectedProductId = Guid.Empty;
        ContractMac.CargueDetailId = Guid.Empty;
        Products = new();
        ListMacs = new();

        if (SelectedCategoryId == Guid.Empty)
        {
            return;
        }

        await LoadProducts(SelectedCategoryId);
    }

    private async Task LoadProducts(Guid productCategoryId)
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>($"{BaseComboProducts}/{productCategoryId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        Products = responseHttp.Response;
    }

    private async Task ProductChanged(ChangeEventArgs e)
    {
        SelectedProductId = Guid.TryParse(e.Value?.ToString(), out var productId) ? productId : Guid.Empty;
        ContractMac.CargueDetailId = Guid.Empty;
        ListMacs = new();

        if (SelectedProductId == Guid.Empty)
        {
            return;
        }

        await LoadMacs(SelectedProductId);
    }

    private async Task LoadMacs(Guid productId)
    {
        //Al editar se pide con el id para que la suya venga en la lista aunque este tomada
        var url = IsEditControl
            ? $"{BaseComboMacs}/{productId}/{ContractMac.CargueDetailId}"
            : $"{BaseComboMacs}/{productId}";

        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        ListMacs = responseHttp.Response;
    }

    private void MacsChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var macid))
        {
            ContractMac.CargueDetailId = macid;
        }
    }
}
