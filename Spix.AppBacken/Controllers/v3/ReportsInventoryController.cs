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
using Spix.DomainLogic.EnumTypes;
using Spix.xFiles.ExcelHelper;

namespace Spix.AppBack.Controllers.v3;

//El reporte de los seriales, en la version de reportes: no toca los modulos de inventario
[ApiVersion("3.0")]
[Route("api/v{version:apiVersion}/reports-inventory")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class ReportsInventoryController : ControllerBase
{
    private readonly IReportInventoryServiceX _reportInventoryService;
    private readonly IReportStockServiceX _reportStockService;
    private readonly IExcelExporter _excelExporter;
    private readonly IStringLocalizer _localizer;

    public ReportsInventoryController(IReportInventoryServiceX reportInventoryService,
        IReportStockServiceX reportStockService, IExcelExporter excelExporter,
        IStringLocalizer localizer)
    {
        _reportInventoryService = reportInventoryService;
        _reportStockService = reportStockService;
        _excelExporter = excelExporter;
        _localizer = localizer;
    }

    //Los estados del serial para el filtro, armados en el backend con su neutro
    [HttpGet("serials/states")]
    public IActionResult GetSerialStates()
    {
        return ResponseHelper.Format(_reportInventoryService.SerialStatesCombo());
    }

    //El DETALLE de los seriales: uno por fila, con su estado y de quien es si esta instalado
    [HttpGet("serials/detail")]
    public async Task<IActionResult> GetSerialDetailAsync(SerialStateType? estado, Guid? productId, Guid? storageId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportInventoryService.GetSerialDetailAsync(userClaimsInfo.UserName, estado, productId, storageId);
        return ResponseHelper.Format(response);
    }

    //El mismo detalle, pero en Excel. Los encabezados salen de los [Display] del DTO.
    [HttpGet("serials/detail/excel")]
    public async Task<IActionResult> GetSerialDetailExcelAsync(SerialStateType? estado, Guid? productId, Guid? storageId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportInventoryService.GetSerialDetailAsync(userClaimsInfo.UserName, estado, productId, storageId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        var archivo = _excelExporter.ExportToExcel(response.Result ?? Enumerable.Empty<ReportSerialDetailDto>());

        return File(archivo,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"seriales-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
    }

    //Los numeros de arriba del reporte de movimientos
    [HttpGet("stock/summary")]
    public async Task<IActionResult> GetStockSummaryAsync(DateTime? desde, DateTime? hasta, Guid? storageId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportStockService.GetSummaryAsync(userClaimsInfo.UserName, desde, hasta, storageId);
        return ResponseHelper.Format(response);
    }

    //Que entro y que salio de cada bodega, deducido de compras y traslados cerrados
    [HttpGet("stock/moves")]
    public async Task<IActionResult> GetStockMovesAsync(DateTime? desde, DateTime? hasta, Guid? storageId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportStockService.GetMovesAsync(userClaimsInfo.UserName, desde, hasta, storageId);
        return ResponseHelper.Format(response);
    }

    //Lo que hay HOY por bodega y producto, con el desglose de seriales
    [HttpGet("stock/balance")]
    public async Task<IActionResult> GetStockBalanceAsync(Guid? storageId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportStockService.GetBalanceAsync(userClaimsInfo.UserName, storageId);
        return ResponseHelper.Format(response);
    }

    //Los totales de seriales de la corporacion
    [HttpGet("serials/summary")]
    public async Task<IActionResult> GetSerialSummaryAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportInventoryService.GetSerialSummaryAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Una fila por producto
    [HttpGet("serials")]
    public async Task<IActionResult> GetSerialsAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportInventoryService.GetSerialsAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
