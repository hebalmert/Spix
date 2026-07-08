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
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.Pagination;
using System.Security.Claims;

namespace Spix.AppBack.Controllers.EntitiesInven
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/purchases")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseServiceX _purchaseUnitOfWork;
        private readonly IStringLocalizer _localizer;

        public PurchasesController(IPurchaseServiceX purchaseUnitOfWork, IStringLocalizer localizer)
        {
            _purchaseUnitOfWork = purchaseUnitOfWork;
            _localizer = localizer;
        }

        [HttpGet("loadComboStatus")]
        public async Task<ActionResult<IEnumerable<IntItemModel>>> GetCombo()
        {
            var response = await _purchaseUnitOfWork.GetComboStatus();
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpGet]
        public async Task<IActionResult> GetSerialsAll([FromQuery] PaginationDTO pagination)
        {
            try
            {
                ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
                var response = await _purchaseUnitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
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
            var response = await _purchaseUnitOfWork.GetAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpPut]
        public async Task<ActionResult<Purchase>> PutAsync(Purchase modelo)
        {
            var response = await _purchaseUnitOfWork.UpdateAsync(modelo);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpPost]
        public async Task<ActionResult<Purchase>> PostAsync(Purchase modelo)
        {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);

var response = await _purchaseUnitOfWork.AddAsync(modelo, userClaimsInfo.UserName);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteAsync(Guid id)
        {
            var response = await _purchaseUnitOfWork.DeleteAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }
    }
}