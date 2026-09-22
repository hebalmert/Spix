using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBack.Controllers.EntitiesNet;

//Mapa de nodos: controlador propio y de solo lectura, aparte de Nodos y de Contratos
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/nodemap")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class NodeMapController : ControllerBase
{
    private readonly INodeMapServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public NodeMapController(
        INodeMapServiceX unitOfWork,
        IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    [HttpGet("nodes")]
    public async Task<IActionResult> GetNodesAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.ComboNodesAsync(userClaimsInfo.UserName);
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

    [HttpGet("views")]
    public async Task<IActionResult> GetViewsAsync()
    {
        try
        {
            var response = await _unitOfWork.ComboViewsAsync();
            return ResponseHelper.Format(response);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }

    [HttpGet("coverages")]
    public async Task<IActionResult> GetCoveragesAsync()
    {
        try
        {
            var response = await _unitOfWork.ComboCoveragesAsync();
            return ResponseHelper.Format(response);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }

    [HttpGet("{nodeId}")]
    public async Task<IActionResult> GetAsync(Guid nodeId)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.GetAsync(nodeId, userClaimsInfo.UserName);
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
