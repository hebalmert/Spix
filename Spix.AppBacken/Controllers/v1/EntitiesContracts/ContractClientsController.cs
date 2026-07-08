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
    [Route("api/v{version:apiVersion}/contractclients")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    [ApiController]
    public class ContractClientsController : ControllerBase
    {
        private readonly IContractClientServiceX _contractClientUnitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;

        public ContractClientsController(IContractClientServiceX contractClientUnitOfWork, IConfiguration configuration,
            IStringLocalizer localizer)
        {
            _contractClientUnitOfWork = contractClientUnitOfWork;
            _configuration = configuration;
            _localizer = localizer;
        }

        [HttpGet("loadStatus")]
        public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboStatus()
        {
            var response = await _contractClientUnitOfWork.GetComboStatusAsync();
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContractClient>>> GetAll([FromQuery] PaginationDTO pagination)
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            if (userClaimsInfo == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _contractClientUnitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var response = await _contractClientUnitOfWork.GetAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }

        [HttpPut]
        public async Task<ActionResult<ContractClient>> PutAsync(ContractClient modelo)
        {
            var response = await _contractClientUnitOfWork.UpdateAsync(modelo);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpPost]
        public async Task<ActionResult<ContractClient>> PostAsync(ContractClient modelo)
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            if (userClaimsInfo == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _contractClientUnitOfWork.AddAsync(modelo, userClaimsInfo.UserName);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteAsync(Guid id)
        {
            var response = await _contractClientUnitOfWork.DeleteAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }
    }
}