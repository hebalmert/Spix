using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.DocumentTypePage;

public partial class CreateDocumentType
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    private DocumentType DocumentType = new();

    private string BaseUrl = "/api/v1/documenttypes";
    private bool IsVisible = false;

    protected override void OnInitialized()
    {
        DocumentType.Active = true;
    }

    private async Task Create()
    {
        IsVisible = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", DocumentType);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            IsVisible = false;
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        IsVisible = false;
        await _modalService.CloseAsync(ModalResult.Ok());
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}