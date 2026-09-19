using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesSecure;

namespace Spix.AppBack.Controllers.v1;

//Revisa y propone nombres de usuario para login. El usuario es unico en TODA la aplicacion,
//por eso lo consultan los formularios de Usuarios, Tecnicos, Contratistas, Clientes y Managers.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/usernames")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrator, Auxiliar")]
[ApiController]
public class UserNamesController : ControllerBase
{
    private readonly IUserNameServiceX _userNameService;

    public UserNamesController(IUserNameServiceX userNameService)
    {
        _userNameService = userNameService;
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckAsync([FromQuery] string userName)
    {
        var response = await _userNameService.CheckAsync(userName ?? string.Empty);
        return ResponseHelper.Format(response);
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> SuggestAsync([FromQuery] string firstName, [FromQuery] string lastName)
    {
        var response = await _userNameService.SuggestAsync(firstName ?? string.Empty, lastName ?? string.Empty);
        return ResponseHelper.Format(response);
    }
}
