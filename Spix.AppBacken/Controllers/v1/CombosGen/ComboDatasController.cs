using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceEntities;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesData;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.ItemsGeneric;

namespace Spix.AppBacken.Controllers.v1.CombosGen;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/combosData")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class ComboDatasController : ControllerBase
{
    private readonly ISecurityServiceX _securityUnitOfWork;
    private readonly IOperationServiceX _operationUnitOfWork;
    private readonly IHotSpotTypeServiceX _hotSpotTypeUnitOfWork;
    private readonly IFrecuencyTypeServiceX _frecuencyTypeUnitOfWork;
    private readonly IFrecuencyServiceX _frecuencyUnitOfWork;
    private readonly IChannelServiceX _channelUnitOfWork;
    private readonly IStateServiceX _stateUnitOfWork;
    private readonly ICityServiceX _cityUnitOfWork;
    private readonly IDocumentTypeServiceX _documentTypeUnitOfWork;
    private readonly ITaxServiceX _taxServiceX;
    private readonly IChainTypesServiceX _chainTypesUnitOfWork;
    private readonly IProductCategoryServiceX _productCategory;
    private readonly IProductServiceX _productService;
    private readonly IPlanServiceX _planServiceX;
    private readonly IStringLocalizer _localizer;

    public ComboDatasController(ISecurityServiceX securityUnitOfWork, IOperationServiceX operationUnitOfWork,
        IHotSpotTypeServiceX hotSpotTypeUnitOfWork, IFrecuencyTypeServiceX frecuencyTypeUnitOfWork,
        IFrecuencyServiceX frecuencyUnitOfWork, IChannelServiceX channelUnitOfWork, IStateServiceX stateUnitOfWork,
        ICityServiceX cityUnitOfWork, IDocumentTypeServiceX documentTypeUnitOfWork, ITaxServiceX taxServiceX,
                IProductCategoryServiceX productCategory, IProductServiceX productService, IPlanServiceX planServiceX,
        IChainTypesServiceX chainTypesUnitOfWork, IStringLocalizer localizer)
    {
        _securityUnitOfWork = securityUnitOfWork;
        _operationUnitOfWork = operationUnitOfWork;
        _hotSpotTypeUnitOfWork = hotSpotTypeUnitOfWork;
        _frecuencyTypeUnitOfWork = frecuencyTypeUnitOfWork;
        _frecuencyUnitOfWork = frecuencyUnitOfWork;
        _channelUnitOfWork = channelUnitOfWork;
        _stateUnitOfWork = stateUnitOfWork;
        _cityUnitOfWork = cityUnitOfWork;
        _documentTypeUnitOfWork = documentTypeUnitOfWork;
        _taxServiceX = taxServiceX;
        _chainTypesUnitOfWork = chainTypesUnitOfWork;
        _productCategory = productCategory;
        _productService = productService;
        _planServiceX = planServiceX;
        _localizer = localizer;
    }

    [HttpGet("ComboUp")]
    public async Task<IActionResult> GetComboUpAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _planServiceX.GetComboUpAsync();
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }

    [HttpGet("ComboDown")]
    public async Task<IActionResult> GetComboDownAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _planServiceX.GetComboDownAsync();
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }

    [HttpGet("ComboProducts/{id}")]
    public async Task<IActionResult> GetComboAsync(Guid id)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _productService.ComboAsync(userClaimsInfo.UserName, id);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }

    [HttpGet("ComboProCategory")]
    public async Task<IActionResult> GetComboProCategoryAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _productCategory.ComboAsync(userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }

    [HttpGet("ComboCity/{id}")]
    public async Task<IActionResult> GetComboCity(int id)
    {
        try
        {
            //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
            //ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer);
            var response = await _cityUnitOfWork.ComboAsync(id);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("ComboState")]
    public async Task<IActionResult> GetComboState(int id)
    {
        try
        {
            //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _stateUnitOfWork.ComboAsync(userClaimsInfo);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("ComboDocumentType")]
    public async Task<IActionResult> GetComboAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _documentTypeUnitOfWork.ComboAsync(userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }

    [HttpGet("ComboSecurity")]
    public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboSecurityAsync()
    {
        var response = await _securityUnitOfWork.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("ComboOperation")]
    public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboOperationAsync()
    {
        var response = await _operationUnitOfWork.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("ComboHotspot")]
    public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboHotspotAsync()
    {
        var response = await _hotSpotTypeUnitOfWork.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("ComboFrecuencyType")]
    public async Task<ActionResult<IEnumerable<FrecuencyType>>> GetComboFrecuencyTypeAsync()
    {
        var response = await _frecuencyTypeUnitOfWork.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("ComboFrecuency/{id:int}")]
    public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboFrecuencyAsync(int id)
    {
        var response = await _frecuencyUnitOfWork.ComboAsync(id);
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("ComboChannel")]
    public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboChannelAsync()
    {
        var response = await _channelUnitOfWork.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("ComboChainType")]
    public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboChainTypeAsync()
    {
        var response = await _chainTypesUnitOfWork.ComboAsync();
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("ComboTaxes")]  //CorporationId
    public async Task<IActionResult> GetComboTaxesAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _taxServiceX.ComboAsync(userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }
}
