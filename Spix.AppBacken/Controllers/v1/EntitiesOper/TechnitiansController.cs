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
    [Route("api/v{version:apiVersion}/technitians")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    [ApiController]
    public class TechnitiansController : ControllerBase
    {
        private readonly ITechnitianServiceX _technitianService;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;

        public TechnitiansController(ITechnitianServiceX technitianService, IConfiguration configuration, IStringLocalizer localizer)
        {
            _technitianService = technitianService;
            _configuration = configuration;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contractor>>> GetAll([FromQuery] PaginationDTO pagination)
        {
            string email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
            if (email == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _technitianService.GetAsync(pagination, email);
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var response = await _technitianService.GetAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }

        [HttpPut]
        public async Task<ActionResult<Technician>> PutAsync(Technician modelo)
        {
            var response = await _technitianService.UpdateAsync(modelo, _configuration["UrlFrontend"]!);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }

        [HttpPost]
        public async Task<ActionResult<Technician>> PostAsync(Technician modelo)
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            if (userClaimsInfo == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _technitianService.AddAsync(modelo, userClaimsInfo.UserName, _configuration["UrlFrontend"]!);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("{id}/re-email")]
        public async Task<ActionResult<bool>> ResendActivationEmailAsync(Guid id)
        {
            var response = await _technitianService.ResendActivationEmailAsync(id, _configuration["UrlFrontend"]!);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Message);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteAsync(Guid id)
        {
            var response = await _technitianService.DeleteAsync(id);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return NotFound(response.Message);
        }
    }
}
