using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;
using System.Security.Claims;

namespace Spix.AppBack.Controllers.EntitiesInven;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/transferDetails")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class TransferDetailsController : ControllerBase
{
    private readonly ITransferDetailsServiceX _transferDetailsUnitOfWork;
    private readonly IStringLocalizer _localizer;

    public TransferDetailsController(ITransferDetailsServiceX transferDetailsUnitOfWork, IStringLocalizer localizer)
    {
        _transferDetailsUnitOfWork = transferDetailsUnitOfWork;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetSerialsAll([FromQuery] PaginationDTO pagination)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.GetAsync(id, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }

    [HttpPut]
    public async Task<ActionResult<TransferDetails>> PutAsync(TransferDetails modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.UpdateAsync(modelo, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }

    [HttpPost]
    public async Task<ActionResult<TransferDetails>> PostAsync(TransferDetails modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);

        var response = await _transferDetailsUnitOfWork.AddAsync(modelo, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }

    [HttpPost("CerrarTrans")]
    public async Task<ActionResult<Transfer>> PostCerrarTransAsyncAsync(Transfer modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);

        var response = await _transferDetailsUnitOfWork.CerrarTransAsync(modelo, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.DeleteAsync(id, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }

    //Los seriales que se pueden elegir para una linea: disponibles y en la bodega de origen
    [HttpGet("serials/available")]
    public async Task<IActionResult> GetAvailableSerialsAsync(Guid transferId, Guid productId, Guid? transferDetailsId)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.GetAvailableSerialsAsync(transferId, productId, transferDetailsId, userClaimsInfo.UserName);
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

    //Los que ya tiene reservados esa linea
    [HttpGet("serials/line/{transferDetailsId}")]
    public async Task<IActionResult> GetLineSerialsAsync(Guid transferDetailsId)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.GetLineSerialsAsync(transferDetailsId, userClaimsInfo.UserName);
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

    //Guarda que equipos van en la linea. La cantidad pasa a ser cuantos se eligieron.
    [HttpPost("serials/{transferDetailsId}")]
    public async Task<IActionResult> SaveSerialsAsync(Guid transferDetailsId, List<Guid> serialIds)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.SaveSerialsAsync(transferDetailsId, serialIds ?? new List<Guid>(), userClaimsInfo.UserName);
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
