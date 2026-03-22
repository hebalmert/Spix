using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.SupplierPage;

public partial class FormSupplier
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private List<DocumentType>? DocumentTypes;
    private List<State>? States;
    private List<City>? Cities = new();
    private string? ImageUrl;
    private string BaseComboState = "/api/v1/combosData/ComboState";
    private string BaseComboCity = "/api/v1/combosData/ComboCity";
    private string BaseView = "/suppliers";
    private string BaseComboDocument = "/api/v1/combosData/ComboDocumentType";

    [Parameter, EditorRequired] public Supplier Supplier { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadDocument();
        await LoadState();
        if (IsEditControl)
        {
            ImageUrl = Supplier.ImageFullPath;
        }
    }

    private async Task LoadDocument()
    {
        var responseHttp = await _repository.GetAsync<List<DocumentType>>($"{BaseComboDocument}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        DocumentTypes = responseHttp.Response;
    }

    private void DocumentChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            Supplier.DocumentTypeId = selectedId;
        }
    }

    private async Task LoadState()
    {
        var responseHttp = await _repository.GetAsync<List<State>>($"{BaseComboState}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        States = responseHttp.Response;
        if (IsEditControl)
        {
            await LoadCity(Supplier!.StateId);
        }
    }

    private async Task StateChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int selectedId))
        {
            Supplier.StateId = selectedId;
            Cities = new();
            await LoadCity(selectedId);
        }
    }

    private async Task LoadCity(int id)
    {
        var responseHttp = await _repository.GetAsync<List<City>>($"{BaseComboCity}/{id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Cities = responseHttp.Response;
    }

    private void CityChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int selectedId))
        {
            Supplier.CityId = selectedId;
        }
    }

    private void ImageSelected(string imagenBase64)
    {
        Supplier.ImgBase64 = imagenBase64;
        ImageUrl = null;
    }
}