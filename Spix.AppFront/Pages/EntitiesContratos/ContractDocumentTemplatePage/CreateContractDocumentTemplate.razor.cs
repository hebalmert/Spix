using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;

public partial class CreateContractDocumentTemplate
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    private ContractDocumentTemplate Model = new();
    private bool IsSaving;
    private const string BaseUrl = "api/v1/contractdocuments/templates";

    protected override void OnInitialized()
    {
        Model.Active = true;
        Model.PageCount = 1;
        Model.DocumentType = ContractDocumentType.ConsentData;
    }

    private async Task Create()
    {
        if (string.IsNullOrWhiteSpace(Model.FileBase64))
        {
            await _sweetAlert.FireAsync("Validacion", "Debe seleccionar un PDF.", SweetAlertIcon.Warning);
            return;
        }

        IsSaving = true;
        var responseHttp = await _repository.PostAsync(BaseUrl, Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Creado", "Plantilla creada correctamente.", SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
