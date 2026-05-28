using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.v1.EntitiesContracts
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/contractcontrols")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    [ApiController]
    public class ContractControlsController : ControllerBase
    {
        private readonly IContractControlServiceX _contractControlUnitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;

        public ContractControlsController(IContractControlServiceX contractControlUnitOfWork, IConfiguration configuration,
            IStringLocalizer localizer)
        {
            _contractControlUnitOfWork = contractControlUnitOfWork;
            _configuration = configuration;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContractClient>>> GetControlContratos([FromQuery] PaginationDTO pagination)
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            if (userClaimsInfo == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _contractControlUnitOfWork.GetControlContratos(pagination, userClaimsInfo.UserName);
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var response = await _contractControlUnitOfWork.GetAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }

    }
}