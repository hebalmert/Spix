using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesData;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesOper.ClientPage;

public partial class FormClient
{
    //Services

    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    //Parameters

    [Parameter, EditorRequired] public Client Client { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private string? ImageUrl;
    private List<DocumentType>? DocumentTypes = new();
    private string BaseView = "/clients";
    private string BaseComboDocument = "/api/v1/combosData/ComboDocumentType";

    protected override async Task OnInitializedAsync()
    {
        await LoadDocuemntType();
        if (IsEditControl)
        {
            ImageUrl = Client.ImageFullPath;
        }
    }

    private async Task LoadDocuemntType()
    {
        var responseHttp = await _repository.GetAsync<List<DocumentType>>($"{BaseComboDocument}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        DocumentTypes = responseHttp.Response;

        if (Client.DocumentTypeId == Guid.Empty && DocumentTypes?.Count > 0)
        {
            Client.DocumentTypeId = DocumentTypes[0].DocumentTypeId;
        }
    }

    private async Task DocuementTypeChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var docuemntType))
        {
            Client.DocumentTypeId = docuemntType;
        }
    }

    private void ImageSelected(string imagenBase64)
    {
        Client.ImgBase64 = imagenBase64;
        ImageUrl = null;
    }
}
