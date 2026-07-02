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

namespace Spix.AppFront.Pages.EntitiesOper.TechnitianPage;

public partial class FormTechnitian
{
    //Services

    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    //Parameters

    [Parameter, EditorRequired] public Technician Technician { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private string? ImageUrl;
    private List<DocumentType>? DocumentTypes = new();
    private string BaseView = "/contractors";
    private string BaseComboDocument = "/api/v1/combosData/ComboDocumentType";

    protected override async Task OnInitializedAsync()
    {
        await LoadDocuemntType();
        if (IsEditControl)
        {
            ImageUrl = Technician.ImageFullPath;
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

        if (!IsEditControl && Technician.DocumentTypeId == Guid.Empty && DocumentTypes?.Count > 0)
        {
            Technician.DocumentTypeId = DocumentTypes[0].DocumentTypeId;
        }
    }

    private async Task DocuementTypeChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var docuemntType))
        {
            Technician.DocumentTypeId = docuemntType;
        }
    }

    private void ImageSelected(string imagenBase64)
    {
        Technician.ImgBase64 = imagenBase64;
        ImageUrl = null;
    }
}
