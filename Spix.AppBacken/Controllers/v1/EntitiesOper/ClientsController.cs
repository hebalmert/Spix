using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spix.AppServiceX.InterfacesOper;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.Pagination;
using System.Security.Claims;

namespace Spix.AppBack.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/clients")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Usuario")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientServiceX _clientServiceX;
        private readonly IConfiguration _configuration;

        public ClientsController(IClientServiceX clientServiceX, IConfiguration configuration)
        {
            _clientServiceX = clientServiceX;
            _configuration = configuration;
        }

        [HttpGet("loadCombo")]
        public async Task<ActionResult<IEnumerable<Client>>> GetComboAsync(int id)
        {
            string email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
            if (email == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _clientServiceX.ComboAsync(email);
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll([FromQuery] PaginationDTO pagination)
        {
            string email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
            if (email == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _clientServiceX.GetAsync(pagination, email);
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
            string email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
            if (email == null)
            {
                return BadRequest("Erro en el sistema de Usuarios");
            }

            var response = await _clientServiceX.AddAsync(modelo, email, _configuration["UrlFrontend"]!);
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