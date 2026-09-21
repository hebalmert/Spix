using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBacken.Controllers.v1.EntitiesSchedule;

//Las fotos de la visita. En su propio controlador porque tocan blobs: subir, firmar la
//url y borrar el archivo es otra responsabilidad, con su propio contenedor.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/servicerequestphotos")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar, Technician")]
[ApiController]
public class ServiceRequestPhotosController : ControllerBase
{
    private readonly IServiceRequestPhotoServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public ServiceRequestPhotosController(IServiceRequestPhotoServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(ServiceRequestPhotoDto dto)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.AddPhotoAsync(dto, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.DeletePhotoAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
