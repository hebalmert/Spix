using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;

//Solo datos de la plantilla (nombre, tipo, activo, PDF). Los campos se editan en FieldsContractDocumentTemplate.
public partial class EditContractDocumentTemplate
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private ContractDocumentTemplate? Model;
    private bool isLoading;
    private bool IsSaving;
    private const string BaseUrl = "api/v1/contractdocuments";

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        isLoading = true;
        var responseHttp = await _repository.GetAsync<ContractDocumentTemplate>($"{BaseUrl}/templates/{Id}");
        isLoading = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response!;

        //Los campos no viajan en el PUT de la plantilla
        Model.ContractDocumentTemplateFields = null;
    }

    private async Task Update()
    {
        if (Model is null)
            return;

        IsSaving = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}/templates", Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Guardado", "Plantilla actualizada correctamente.", SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
