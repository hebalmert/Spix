using Spix.DomainLogic.AppResponses;

namespace Spix.AppWpf.Services.Auth;

// Define el acceso al Backend sin acoplar las ventanas al repositorio HTTP.
public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(LoginDTO loginDTO);
}
