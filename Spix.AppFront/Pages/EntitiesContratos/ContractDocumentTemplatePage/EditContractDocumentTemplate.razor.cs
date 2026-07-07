using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;

public partial class EditContractDocumentTemplate
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private ContractDocumentTemplate? Model;
    private List<ContractDocumentTemplateField> Fields = new();
    private ContractDocumentTemplateField NewField = new();
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
        Fields = Model.ContractDocumentTemplateFields?.OrderBy(x => x.PageNumber).ThenBy(x => x.FieldType).ToList() ?? new();
        ResetNewField();
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

    private async Task AddField(ContractDocumentTemplateField field)
    {
        if (Model is null)
            return;

        if (field.PageNumber < 1 || field.PageNumber > Model.PageCount)
        {
            await _sweetAlert.FireAsync("Validacion", "La pagina esta fuera del rango del PDF.", SweetAlertIcon.Warning);
            return;
        }

        field.ContractDocumentTemplateId = Model.ContractDocumentTemplateId;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/fields", field);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await LoadAsync();
    }

    private async Task DeleteField(ContractDocumentTemplateField field)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Eliminar",
            Text = "Desea eliminar esta coordenada?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Eliminar",
            CancelButtonText = "Cancelar"
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/fields/{field.ContractDocumentTemplateFieldId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await LoadAsync();
    }

    private void ResetNewField()
    {
        NewField = new ContractDocumentTemplateField
        {
            FieldType = ContractDocumentFieldType.FullName,
            PageNumber = 1,
            FontSize = 12,
            Width = 200,
            Height = 60
        };
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
