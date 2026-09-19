using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;
using System.Security.Cryptography;

namespace Spix.AppFront.Pages.EntitiesContratos.MySignaturePage;

//Certificado de una firma. Recibe el identificador y consulta el API; la huella del archivo
//se calcula en el navegador, el PDF nunca se envia.
public partial class SignatureCertificate
{
    [Inject] private IRepository _repository { get; set; } = null!;

    [Parameter] public string? Code { get; set; }

    private const string BaseUrl = "api/v1/signatureverification";
    private const long MaxFileSize = 25 * 1024 * 1024;

    private SignatureVerificationDTO? result;
    private bool isLoading;
    private string? loadedCode;

    private bool fileChecked;
    private bool fileMatches;
    private string? fileHash;

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(Code) || string.Equals(Code, loadedCode, StringComparison.OrdinalIgnoreCase))
            return;

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loadedCode = Code;
        isLoading = true;
        fileChecked = false;
        fileHash = null;
        await InvokeAsync(StateHasChanged);

        var responseHttp = await _repository.GetAsync<SignatureVerificationDTO>($"{BaseUrl}/{Code!.Trim().ToUpperInvariant()}");

        //Aqui no se usa el manejador global: el resultado se responde en la misma pantalla
        result = responseHttp.Error
            ? new SignatureVerificationDTO { Found = false, VerificationCode = Code! }
            : responseHttp.Response;

        isLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task CheckFileAsync(InputFileChangeEventArgs e)
    {
        using var stream = e.File.OpenReadStream(MaxFileSize);
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        fileHash = Convert.ToHexString(SHA256.HashData(memory.ToArray()));
        fileMatches = string.Equals(fileHash, result?.FileHash, StringComparison.OrdinalIgnoreCase);
        fileChecked = true;
    }

    private static string GetEventName(SignatureEventType eventType) => eventType switch
    {
        SignatureEventType.RequestSent => "Solicitud de firma enviada al correo",
        SignatureEventType.DocumentViewed => "El firmante abrio el documento",
        SignatureEventType.CodeSent => "Codigo de verificacion enviado al correo",
        SignatureEventType.CodeFailed => "Codigo incorrecto",
        SignatureEventType.CodeValidated => "Codigo validado",
        SignatureEventType.Signed => "Documento firmado",
        _ => eventType.ToString()
    };
}
