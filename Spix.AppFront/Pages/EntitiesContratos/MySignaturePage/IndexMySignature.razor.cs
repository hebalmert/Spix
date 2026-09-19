using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.MySignaturePage;

//Portal del cliente: sus documentos para firmar (Consentimiento y Contrato)
public partial class IndexMySignature
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private const string BaseUrl = "api/v1/mysignatures";

    private List<MySignatureDocumentDTO> documents = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        isLoading = true;
        await InvokeAsync(StateHasChanged);

        var responseHttp = await _repository.GetAsync<List<MySignatureDocumentDTO>>(BaseUrl);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            isLoading = false;
            await InvokeAsync(StateHasChanged);
            return;
        }

        documents = responseHttp.Response ?? new List<MySignatureDocumentDTO>();
        isLoading = false;

        //Recargar viene del cierre del modal, que no es un evento de Blazor: hay que pintar a mano
        await InvokeAsync(StateHasChanged);
    }

    private async Task OpenSignAsync(MySignatureDocumentDTO item)
    {
        var parameters = new Dictionary<string, object>
        {
            { "ContractClientId", item.ContractClientId },
            { "DocumentType", item.DocumentType },
            { "Title", $"{GetDocumentName(item.DocumentType)} - Contrato #{item.ContractNumber}" }
        };

        await _modalService.ShowAsync(typeof(SignMyDocument), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadAsync();
        });
    }

    //Documento ya firmado: se abre solo para leerlo
    private async Task OpenViewAsync(MySignatureDocumentDTO item)
    {
        var parameters = new Dictionary<string, object>
        {
            { "ContractClientId", item.ContractClientId },
            { "DocumentType", item.DocumentType },
            { "Title", $"{GetDocumentName(item.DocumentType)} - Contrato #{item.ContractNumber}" }
        };

        await _modalService.ShowAsync(typeof(ViewMyDocument), parameters);
    }

    //Certificado e historial de la firma: la misma informacion a la que lleva el QR
    private async Task OpenCertificateAsync(MySignatureDocumentDTO item)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Code", item.VerificationCode! },
            { "Title", $"{GetDocumentName(item.DocumentType)} - Contrato #{item.ContractNumber}" }
        };

        await _modalService.ShowAsync(typeof(CertificateMyDocument), parameters);
    }

    private static string GetDocumentName(ContractDocumentType documentType) =>
        documentType == ContractDocumentType.Contract ? "Contrato de servicio" : "Consentimiento de datos";
}
