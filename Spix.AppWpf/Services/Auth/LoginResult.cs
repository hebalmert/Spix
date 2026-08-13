using Spix.DomainLogic.AppResponses;

namespace Spix.AppWpf.Services.Auth;

// Representa el resultado que la interfaz necesita despues de solicitar el acceso.
public class LoginResult
{
    public bool WasSuccess { get; init; }

    public string? Message { get; init; }

    public TokenDTO? TokenResponse { get; init; }
}
