using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.MySignaturePage;

//Firma electronica del cliente: leer -> aceptar -> codigo al correo -> firmar
//(ver docs/Firma-Electronica-Part11.md)
public partial class SignMyDocument
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    [Parameter, EditorRequired] public Guid ContractClientId { get; set; }
    [Parameter, EditorRequired] public ContractDocumentType DocumentType { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/mysignatures";
    private const string CanvasId = "mySignatureCanvas";

    private bool termsAccepted;
    private bool codeSent;
    private bool isSending;
    private bool isSaving;
    private bool canvasInitialized;
    private string code = string.Empty;
    private string maskedEmail = string.Empty;
    private DateTime? expiresAt;
    private string termsText = string.Empty;
    private string termsVersion = string.Empty;
    private string? fileUrl;
    private bool isLoadingFile = true;

    protected override async Task OnInitializedAsync()
    {
        //El enlace del PDF se pide aqui, al abrir: dura pocos minutos y queda en la bitacora
        var linkHttp = await _repository.GetAsync<SignatureLinkDTO>($"{BaseUrl}/link/{ContractClientId}/{DocumentType}");
        if (!await _responseHandler.HandleErrorAsync(linkHttp))
            fileUrl = linkHttp.Response?.Url;

        isLoadingFile = false;

        var responseHttp = await _repository.GetAsync<SignatureTermsDTO>($"{BaseUrl}/terms");
        if (responseHttp.Error || responseHttp.Response == null)
            return;

        termsText = responseHttp.Response.Text;
        termsVersion = responseHttp.Response.Version;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //El lienzo solo existe cuando ya se envio el codigo
        if (codeSent && !canvasInitialized)
        {
            canvasInitialized = true;
            await JS.InvokeVoidAsync("spixSignature.init", CanvasId);
        }
    }

    private void OnTermsChanged(ChangeEventArgs e)
    {
        termsAccepted = e.Value is bool value && value;
    }

    private async Task RequestCodeAsync()
    {
        isSending = true;
        var responseHttp = await _repository.PostAsync<object, SignatureCodeDTO>($"{BaseUrl}/code/{ContractClientId}/{DocumentType}", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            isSending = false;
            return;
        }

        maskedEmail = responseHttp.Response?.MaskedEmail ?? string.Empty;
        expiresAt = responseHttp.Response?.ExpiresAt;
        code = string.Empty;
        codeSent = true;
        isSending = false;

        await _sweetAlert.FireAsync("Codigo enviado", $"Revise su correo {maskedEmail}.", SweetAlertIcon.Success);
    }

    private async Task SignAsync()
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length != 6)
        {
            await _sweetAlert.FireAsync("Firma", "Escriba el codigo de 6 digitos que recibio en su correo.", SweetAlertIcon.Warning);
            return;
        }

        var base64 = await JS.InvokeAsync<string>("spixSignature.getBase64");
        if (string.IsNullOrWhiteSpace(base64))
        {
            await _sweetAlert.FireAsync("Firma", "Debe dibujar su firma.", SweetAlertIcon.Warning);
            return;
        }

        var confirm = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Firmar documento",
            Text = "Al firmar acepta el contenido del documento. Esta accion no se puede deshacer.",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Firmar",
            CancelButtonText = "Cancelar"
        });

        if (confirm.IsDismissed || confirm.Value != "true")
            return;

        var model = new SignDocumentRequestDTO
        {
            ContractClientId = ContractClientId,
            DocumentType = DocumentType,
            Code = code.Trim(),
            SignatureBase64 = base64,
            TermsAccepted = termsAccepted
        };

        isSaving = true;
        var responseHttp = await _repository.PostAsync<SignDocumentRequestDTO, bool>($"{BaseUrl}/sign", model);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            isSaving = false;
            return;
        }

        isSaving = false;
        await _modalService.CloseAsync(ModalResult.Ok());
        await _sweetAlert.FireAsync("Firma", "Documento firmado correctamente.", SweetAlertIcon.Success);
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
