using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;

public partial class SignContractDocument
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    [Parameter] public Guid ContractClientId { get; set; }
    [Parameter] public ContractDocumentType DocumentType { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/contractdocuments";
    private const string CanvasId = "contractSignatureCanvas";
    private ContractSignedDocument? Model;
    private bool isLoading = true;
    private bool IsSaving;
    private bool canvasInitialized;

    protected override async Task OnInitializedAsync()
    {
        await GenerateDocumentAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!canvasInitialized && Model is not null && !Model.Signed)
        {
            canvasInitialized = true;
            await JS.InvokeVoidAsync("spixSignature.init", CanvasId);
        }
    }

    private async Task GenerateDocumentAsync()
    {
        isLoading = true;
        var responseHttp = await _repository.PostAsync<object, ContractSignedDocument>($"{BaseUrl}/generate/{ContractClientId}/type/{DocumentType}", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            isLoading = false;
            return;
        }

        Model = responseHttp.Response;
        isLoading = false;
    }

    private async Task SaveSignature()
    {
        if (Model is null)
            return;

        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Firma",
            Text = "Desea guardar la firma del documento?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Guardar",
            CancelButtonText = "Cancelar"
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var base64 = await JS.InvokeAsync<string>("spixSignature.getBase64");
        if (string.IsNullOrWhiteSpace(base64))
        {
            await _sweetAlert.FireAsync("Firma", "Debe capturar la firma.", SweetAlertIcon.Warning);
            return;
        }

        IsSaving = true;
        Model.SignatureBase64 = base64;
        var responseHttp = await _repository.PostAsync<ContractSignedDocument, ContractSignedDocument>($"{BaseUrl}/sign", Model);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            IsSaving = false;
            return;
        }

        Model = responseHttp.Response;
        IsSaving = false;
        await _sweetAlert.FireAsync("Firma", "Documento firmado correctamente.", SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task ClearSignature()
    {
        await JS.InvokeVoidAsync("spixSignature.clear");
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
