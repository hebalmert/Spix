using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.Domain.EntitiesData;

namespace Spix.AppBacken.Controllers.v1.CombosGen;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/downData")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar, Usuario")]
[ApiController]
public class DownDataController : ControllerBase
{
    private readonly IStringLocalizer _localizer;
    private readonly IOperationServiceX _operationService;
    private readonly IChannelServiceX _channelService;
    private readonly ISecurityServiceX _securityService;
    private readonly IFrecuencyTypeServiceX _frecuencyTypeService;
    private readonly IFrecuencyServiceX _frecuencyService;

    public DownDataController(IStringLocalizer localizer, IOperationServiceX operationService, IChannelServiceX channelService,
        ISecurityServiceX securityService, IFrecuencyTypeServiceX frecuencyTypeService, IFrecuencyServiceX frecuencyService)
    {

        _localizer = localizer;
        _operationService = operationService;
        _channelService = channelService;
        _securityService = securityService;
        _frecuencyTypeService = frecuencyTypeService;
        _frecuencyService = frecuencyService;
    }


    [HttpGet("OperationCombo")]
    public async Task<ActionResult<IEnumerable<Operation>>> GetComboAsync()
    {
        var response = await _operationService.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("channelCombo")]
    public async Task<ActionResult<IEnumerable<Channel>>> GetChannerlAsync()
    {
        var response = await _channelService.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("SecurityCombo")]
    public async Task<ActionResult<IEnumerable<Security>>> GetSecurityAsync()
    {
        var response = await _securityService.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("FreCuentyTypeCombo")]
    public async Task<ActionResult<IEnumerable<FrecuencyType>>> GetFrecuencyTypeComboAsync()
    {
        var response = await _frecuencyTypeService.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("frecuency/{id:int}")]
    public async Task<ActionResult<IEnumerable<Frecuency>>> GetFrecuenyAsync(int id)
    {
        var response = await _frecuencyService.ComboAsync(id);
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }
}
