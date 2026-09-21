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

//Los servicios que se le cargan a una visita. En su propio controlador: cuando entre la
//facturacion esto crece, y no tiene por que engordar el de la solicitud.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/servicerequestdetails")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar, Technician")]
[ApiController]
public class ServiceRequestDetailsController : ControllerBase
{
    private readonly IServiceRequestDetailServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public ServiceRequestDetailsController(IServiceRequestDetailServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(ServiceRequestDetailDto dto)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.AddDetailAsync(dto, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.DeleteDetailAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
