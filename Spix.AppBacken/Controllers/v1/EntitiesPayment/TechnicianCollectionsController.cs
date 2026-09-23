using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesPayment;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.EntitiesPayment;

//El cruce de lo que recogio un tecnico tiene su propio controlador: no carga el de
//Cuentas por Cobrar, que es el que usan la oficina y los tecnicos todo el dia.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/technician-collections")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class TechnicianCollectionsController : ControllerBase
{
    private readonly ITechnicianCollectionServiceX _collectionService;
    private readonly IStringLocalizer _localizer;

    public TechnicianCollectionsController(ITechnicianCollectionServiceX collectionService, IStringLocalizer localizer)
    {
        _collectionService = collectionService;
        _localizer = localizer;
    }

    //Quienes han recibido pagos
    [HttpGet("combocollectors")]
    public async Task<IActionResult> ComboCollectorsAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _collectionService.ComboCollectorsAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //El total del periodo: es lo que hay que recibirle
    [HttpGet("summary/{userId:guid}")]
    public async Task<IActionResult> GetSummaryAsync(Guid userId, [FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _collectionService.GetSummaryAsync(userId, pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los cobros del periodo, paginados
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetCollectionsAsync(Guid userId, [FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _collectionService.GetCollectionsAsync(userId, pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
