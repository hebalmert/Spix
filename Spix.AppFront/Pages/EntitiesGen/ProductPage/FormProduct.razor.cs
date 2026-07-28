using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;
namespace Spix.AppFront.Pages.EntitiesGen.ProductPage;

public partial class FormProduct
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private List<GuidItemModel>? Taxes;
    private List<Mark>? Marks = new();
    private List<MarkModel>? MarkModels = new();

    private const string BaseComboMark = "/api/v1/marks/loadCombo";
    private const string BaseComboMarkModel = "/api/v1/marksmodels/loadCombo";

    [Parameter, EditorRequired] public Product Product { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadMarksAsync();
        await LoadTaxes();
    }

    private async Task LoadMarksAsync()
    {
        var responseHttp = await _repository.GetAsync<List<Mark>>(BaseComboMark);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/productcategories");
            return;
        }

        Marks = responseHttp.Response;
        if (IsEditControl)
        {
            await LoadMarkModelsAsync(Product.MarkId ?? Guid.Empty);
        }
    }

    private async Task LoadMarkModelsAsync(Guid markId)
    {
        var responseHttp = await _repository.GetAsync<List<MarkModel>>($"{BaseComboMarkModel}/{markId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/productcategories");
            return;
        }

        MarkModels = responseHttp.Response;
    }

    private async Task MarkChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var markId))
        {
            Product.MarkId = markId;
            Product.MarkModelId = Guid.Empty;
            await LoadMarkModelsAsync(markId);
        }
    }

    private void MarkModelChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var markModelId))
        {
            Product.MarkModelId = markModelId;
        }
    }

    private async Task LoadTaxes()
    {
        var responseHTTP = await _repository.GetAsync<List<GuidItemModel>>($"api/v1/combosData/ComboTaxes");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/products");
            return;
        }

        Taxes = responseHTTP.Response;
    }

    private void TaxChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            Product.TaxId = selectedId;
        }
    }
}
