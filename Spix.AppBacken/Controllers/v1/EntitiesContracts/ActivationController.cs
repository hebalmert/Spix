using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.EntitiesContracts;

//La reactivacion tiene su propio controlador: no carga el de contratos ni el de cobros
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/activation")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class ActivationController : ControllerBase
{
    private readonly IActivationServiceX _activationService;
    private readonly IStringLocalizer _localizer;

    public ActivationController(IActivationServiceX activationService, IStringLocalizer localizer)
    {
        _activationService = activationService;
        _localizer = localizer;
    }

    //Cuantos esperan reactivacion y como quedan repartidos por equipo
    [HttpGet("check")]
    public async Task<IActionResult> CheckAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _activationService.CheckAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los que esperan reactivacion, paginados
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _activationService.GetPendingAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Reactiva todos los contratos de un equipo: una sola conexion Mikrotik
    [HttpPost("run/server/{serverId:guid}")]
    public async Task<IActionResult> RunServerAsync(Guid serverId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _activationService.RunServerAsync(serverId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
