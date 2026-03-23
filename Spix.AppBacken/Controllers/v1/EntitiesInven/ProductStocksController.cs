using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesInven;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EntitiesDTO;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.EntitiesInven
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/productStocks")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    [ApiController]
    public class ProductStocksController : ControllerBase
    {
        private readonly IProductStockServiceX _productStockUnitOfWork;
        private readonly IStringLocalizer _localizer;

        public ProductStocksController(IProductStockServiceX productStockUnitOfWork, IStringLocalizer localizer)
        {
            _productStockUnitOfWork = productStockUnitOfWork;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> GetSerialsAll([FromQuery] PaginationDTO pagination)
        {
            try
            {
                ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
                var response = await _productStockUnitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var response = await _productStockUnitOfWork.GetAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("transferstock")]
        public async Task<IActionResult> GetProductStock([FromQuery] TransferStockDTO modelo)
        {
            var response = await _productStockUnitOfWork.GetProductStock(modelo);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }
    }
}