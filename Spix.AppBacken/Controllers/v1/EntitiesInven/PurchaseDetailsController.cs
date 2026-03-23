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
    [Route("api/v{version:apiVersion}/purchaseDetails")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    [ApiController]
    public class PurchaseDetailsController : ControllerBase
    {
        private readonly IPurchaseDetailsServiceX _purchaseDetailsUnitOfWork;
        private readonly IStringLocalizer _localizer;

        public PurchaseDetailsController(IPurchaseDetailsServiceX purchaseDetailsUnitOfWork, IStringLocalizer localizer)
        {
            _purchaseDetailsUnitOfWork = purchaseDetailsUnitOfWork;
            _localizer = localizer;
        }

        [HttpGet("loadComboStatus")]
        public async Task<ActionResult<IEnumerable<IntItemModel>>> GetCombo()
        {
            var response = await _purchaseDetailsUnitOfWork.GetComboStatus();
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
                ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
                var response = await _purchaseDetailsUnitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
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
            var response = await _purchaseDetailsUnitOfWork.GetAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpPut]
        public async Task<ActionResult<PurchaseDetail>> PutAsync(PurchaseDetail modelo)
        {
            var response = await _purchaseDetailsUnitOfWork.UpdateAsync(modelo);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseDetail>> PostAsync(PurchaseDetail modelo)
        {
            string email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
            if (email == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _purchaseDetailsUnitOfWork.AddAsync(modelo, email);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("CerrarPurchase")]
        public async Task<ActionResult<Purchase>> PostClosePurchaseAsync(Purchase modelo)
        {
            string email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
            if (email == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _purchaseDetailsUnitOfWork.ClosePurchaseSync(modelo, email);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteAsync(Guid id)
        {
            var response = await _purchaseDetailsUnitOfWork.DeleteAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }
    }
}