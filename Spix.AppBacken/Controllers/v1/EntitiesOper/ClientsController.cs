using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppServiceX.InterfacesOper;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;
using System.Security.Claims;

namespace Spix.AppBack.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/clients")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientServiceX _clientServiceX;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;

        public ClientsController(IClientServiceX clientServiceX, IConfiguration configuration, IStringLocalizer localizer)
        {
            _clientServiceX = clientServiceX;
            _configuration = configuration;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll([FromQuery] PaginationDTO pagination)
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            if (userClaimsInfo == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _clientServiceX.GetAsync(pagination, userClaimsInfo.UserName);
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var response = await _clientServiceX.GetAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }

        [HttpPut]
        public async Task<ActionResult<Client>> PutAsync(Client modelo)
        {
            var response = await _clientServiceX.UpdateAsync(modelo, _configuration["UrlFrontend"]!);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }

        [HttpPost]
        public async Task<ActionResult<Client>> PostAsync(Client modelo)
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            if (userClaimsInfo == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _clientServiceX.AddAsync(modelo, userClaimsInfo.UserName, _configuration["UrlFrontend"]!);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("{id}/re-email")]
        public async Task<ActionResult<bool>> ResendActivationEmailAsync(Guid id)
        {
            var response = await _clientServiceX.ResendActivationEmailAsync(id, _configuration["UrlFrontend"]!);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteAsync(Guid id)
        {
            var response = await _clientServiceX.DeleteAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }
    }
}
