using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EntitiesContractDTO;

namespace Spix.AppBack.Controllers.v2;

//La version v2 del API es la del ESCRITORIO.
//
//El WPF configura el MikroTik por la red LAN, porque el cliente puede no tener IP publica.
//Para mandar las mismas ordenes que manda el Backend necesita saber lo mismo, y parte de
//esos datos no estaban expuestos: si el queue padre ya existe y que IPs cuelgan de el.
//
//Los GET entregan los datos; los POST y el DELETE guardan lo que el escritorio YA escribio
//en el equipo: aqui NUNCA se toca el MikroTik, eso ya paso por la LAN.
//
//Viven en v2 a proposito: asi lo que ya funciona en v1 con Blazor no se toca ni se arriesga.
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/contractmksetup")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class ContractMkSetupController : ControllerBase
{
    private readonly IContractMkSetupServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public ContractMkSetupController(IContractMkSetupServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    // Todo lo que el escritorio necesita para armar la Queue de un contrato
    [HttpGet("que/{contractClientId}")]
    public async Task<IActionResult> GetQueSetupAsync(Guid contractClientId)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.GetQueSetupAsync(contractClientId, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }

    // Lo mismo para el IpBinding
    [HttpGet("bind/{contractClientId}")]
    public async Task<IActionResult> GetBindSetupAsync(Guid contractClientId)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.GetBindSetupAsync(contractClientId, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }

    // De aqui para abajo el equipo YA quedo configurado por la LAN: solo se guarda el registro

    [HttpPost("que")]
    public async Task<IActionResult> SaveQueAsync(ContractQueSaveDTO datos)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.SaveQueAsync(datos, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }

    //Va por POST y no por DELETE porque hay que decir como quedo el queue padre
    [HttpPost("que/remove")]
    public async Task<IActionResult> RemoveQueAsync(ContractQueRemoveDTO datos)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.RemoveQueAsync(datos, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }

    //Sirve para crear y para editar: el IpBinding es el unico que se edita sin quitarlo
    [HttpPost("bind")]
    public async Task<IActionResult> SaveBindAsync(ContractBind modelo)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.SaveBindAsync(modelo, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }

    [HttpDelete("bind/{id}")]
    public async Task<IActionResult> RemoveBindAsync(Guid id)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.RemoveBindAsync(id, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }

    // Como queda el queue padre cuando este cliente se vaya: sin esto no se puede quitar
    [HttpGet("que/remove/{contractQueId}")]
    public async Task<IActionResult> GetQueRemoveSetupAsync(Guid contractQueId)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.GetQueRemoveSetupAsync(contractQueId, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }

    // Con quien hablar: lo usan editar y quitar el IpBinding
    [HttpGet("conn/{contractClientId}")]
    public async Task<IActionResult> GetConnectionAsync(Guid contractClientId)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.GetConnectionAsync(contractClientId, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }
}
