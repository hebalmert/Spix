using Spix.DomainLogic.AppResponses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Spix.AppWpf.Services.Session;

// Conserva la sesion solamente mientras la aplicacion desktop permanece abierta.
public class UserSessionService : IUserSessionService
{
    public TokenDTO? CurrentSession { get; private set; }

    public string? Token => CurrentSession?.Token;

    public string UserName { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string Role { get; private set; } = string.Empty;

    public string CorporationName { get; private set; } = string.Empty;

    public string Initials { get; private set; } = "SP";

    public Task<string?> GetTokenAsync()
    {
        return Task.FromResult(Token);
    }

    public void SetSession(TokenDTO tokenResponse)
    {
        CurrentSession = tokenResponse;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokenResponse.Token);
        UserName = GetClaimValue(jwt, ClaimTypes.Name);
        Role = GetClaimValue(jwt, ClaimTypes.Role);
        CorporationName = GetClaimValue(jwt, "CorpName");
        var firstName = GetClaimValue(jwt, "FirstName");
        var lastName = GetClaimValue(jwt, "LastName");
        FullName = string.Join(
            " ",
            new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value)));

        if (string.IsNullOrWhiteSpace(FullName))
        {
            FullName = UserName;
        }

        Initials = GetInitials(FullName);
    }

    public void ClearSession()
    {
        CurrentSession = null;
        UserName = string.Empty;
        FullName = string.Empty;
        Role = string.Empty;
        CorporationName = string.Empty;
        Initials = "SP";
    }

    // Lee un claim aun cuando el JWT lo almacena usando el URI estandar de .NET.
    private static string GetClaimValue(JwtSecurityToken jwt, string claimType)
    {
        return jwt.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value ?? string.Empty;
    }

    // Genera las iniciales que se muestran en el avatar de la sesion activa.
    private static string GetInitials(string value)
    {
        var words = value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0]));

        var initials = new string(words.ToArray());
        return string.IsNullOrWhiteSpace(initials) ? "SP" : initials;
    }
}
