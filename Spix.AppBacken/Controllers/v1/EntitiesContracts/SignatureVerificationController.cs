using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.DomainLogic.AppResponses;
using Spix.AppServiceX.InterfacesSignature;

namespace Spix.AppBack.Controllers.EntitiesContracts;

//Verificacion de una firma electronica con el identificador impreso en el documento.
//Pide sesion: el QR lleva al usuario a entrar con su cuenta y ahi ve el certificado.
//Solo devuelve datos enmascarados, nunca el PDF.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/signatureverification")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class SignatureVerificationController : ControllerBase
{
    private readonly ISignatureServiceX _signatureService;
    private readonly IStringLocalizer _localizer;

    public SignatureVerificationController(ISignatureServiceX signatureService, IStringLocalizer localizer)
    {
        _signatureService = signatureService;
        _localizer = localizer;
    }

    [HttpGet("{verificationCode}")]
    public async Task<IActionResult> VerifyAsync(string verificationCode)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _signatureService.VerifySignatureAsync(verificationCode, userClaimsInfo);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }
}
