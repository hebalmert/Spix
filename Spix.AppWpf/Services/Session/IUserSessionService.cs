using Spix.DomainLogic.AppResponses;

namespace Spix.AppWpf.Services.Session;

// Expone la sesion vigente para que todos los servicios compartan el mismo JWT.
public interface IUserSessionService
{
    TokenDTO? CurrentSession { get; }

    string? Token { get; }

    string UserName { get; }

    string FullName { get; }

    string Role { get; }

    string CorporationName { get; }

    string Initials { get; }

    Task<string?> GetTokenAsync();

    void SetSession(TokenDTO tokenResponse);

    void ClearSession();
}
