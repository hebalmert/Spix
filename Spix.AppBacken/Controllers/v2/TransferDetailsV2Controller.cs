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

namespace Spix.AppBack.Controllers.v2;

//Las lineas del traslado para el ESCRITORIO. Gemelo de TransfersV2Controller: tambien es
//una puerta delgada sobre el mismo ITransferDetailsServiceX que usa v1.
//
//CerrarTrans es la que mueve el stock de una bodega a la otra y deja el traslado cerrado:
//despues de eso el traslado ya no se edita ni se borra.
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/transferDetails")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class TransferDetailsV2Controller : ControllerBase
{
    private readonly ITransferDetailsServiceX _transferDetailsUnitOfWork;
    private readonly IStringLocalizer _localizer;

    public TransferDetailsV2Controller(ITransferDetailsServiceX transferDetailsUnitOfWork, IStringLocalizer localizer)
    {
        _transferDetailsUnitOfWork = transferDetailsUnitOfWork;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.GetAsync(id, userClaimsInfo.UserName);
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

    [HttpPost]
    public async Task<IActionResult> PostAsync(TransferDetails modelo)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.AddAsync(modelo, userClaimsInfo.UserName);
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

    [HttpPut]
    public async Task<IActionResult> PutAsync(TransferDetails modelo)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.UpdateAsync(modelo, userClaimsInfo.UserName);
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

    //Cierra el traslado: mueve el stock de la bodega origen a la destino
    [HttpPost("CerrarTrans")]
    public async Task<IActionResult> PostCerrarTransAsync(Transfer modelo)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.CerrarTransAsync(modelo, userClaimsInfo.UserName);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.DeleteAsync(id, userClaimsInfo.UserName);
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
    //Los equipos que YA viajaron en esta linea: sale del historico, no del serial
    [HttpGet("serials/moved/{transferDetailsId}")]
    public async Task<IActionResult> GetMovedSerialsAsync(Guid transferDetailsId)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _transferDetailsUnitOfWork.GetMovedSerialsAsync(transferDetailsId, userClaimsInfo.UserName);
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
