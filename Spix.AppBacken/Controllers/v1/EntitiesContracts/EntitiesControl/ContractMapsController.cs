using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBack.Controllers.EntitiesNet;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contractmaps")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class ContractMapsController : ControllerBase
{
    private readonly IContractMapServiceX _serverUnitOfWork;
    private readonly IStringLocalizer _localizer;

    public ContractMapsController(IContractMapServiceX serverUnitOfWork, IStringLocalizer localizer)
    {
        _serverUnitOfWork = serverUnitOfWork;
        _localizer = localizer;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var response = await _serverUnitOfWork.GetAsync(id);
        if (response.WasSuccess)
            return Ok(response.Result);

        return NotFound(response.Message);
    }

    [HttpPost]
    public async Task<ActionResult<ContractMap>> PostAsync(ContractMap modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _serverUnitOfWork.AddAsync(modelo, userClaimsInfo.UserName);
        if (response.WasSuccess)
            return Ok(response.Result);

        return NotFound(response.Message);
    }

    [HttpPut]
    public async Task<ActionResult<ContractMap>> PutAsync(ContractMap modelo)
    {
        var response = await _serverUnitOfWork.UpdateAsync(modelo);
        if (response.WasSuccess)
            return Ok(response.Result);

        return NotFound(response.Message);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync(Guid id)
    {
        var response = await _serverUnitOfWork.DeleteAsync(id);
        if (response.WasSuccess)
            return Ok(response.Result);

        return NotFound(response.Message);
    }
}
