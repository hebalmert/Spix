using Spix.DomainLogic.AppResponses;
using Spix.HttpService;
using Spix.AppWpf.Services.Session;
using System.Net.Http;

namespace Spix.AppWpf.Services.Auth;

// Ejecuta el mismo endpoint de acceso que utiliza el frontend Blazor.
public class AuthenticationService : IAuthenticationService
{
    private const string LoginUrl = "api/v1/accounts/Login";

    private readonly IRepository _repository;
    private readonly IUserSessionService _sessionService;

    public AuthenticationService(
        IRepository repository,
        IUserSessionService sessionService)
    {
        _repository = repository;
        _sessionService = sessionService;
    }

    public async Task<LoginResult> LoginAsync(LoginDTO loginDTO)
    {
        try
        {
            var responseHttp = await _repository.PostAsync<LoginDTO, TokenDTO>(
                LoginUrl,
                loginDTO);

            if (responseHttp.Error || responseHttp.Response is null)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                return new LoginResult
                {
                    WasSuccess = false,
                    Message = string.IsNullOrWhiteSpace(message)
                        ? "No fue posible iniciar sesion."
                        : message.Trim().Trim('"')
                };
            }

            if (string.IsNullOrWhiteSpace(responseHttp.Response.Token))
            {
                return new LoginResult
                {
                    WasSuccess = false,
                    Message = "El Backend no devolvio un token de acceso valido."
                };
            }

            _sessionService.SetSession(responseHttp.Response);

            return new LoginResult
            {
                WasSuccess = true,
                TokenResponse = responseHttp.Response
            };
        }
        catch (HttpRequestException)
        {
            return new LoginResult
            {
                WasSuccess = false,
                Message = "No fue posible conectar con el Backend. Verifica la URL y la conexion."
            };
        }
        catch (Exception)
        {
            return new LoginResult
            {
                WasSuccess = false,
                Message = "Ocurrio un error al iniciar sesion. Intenta nuevamente."
            };
        }
    }
}
