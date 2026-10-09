using System.IdentityModel.Tokens.Jwt;

namespace Spix.AppMaui.Services;

//La sesion del tecnico. El token va en SecureStorage, que en Android queda cifrado con
//el llavero del sistema: no en Preferences, que se lee en claro.
public class SessionService
{
    private const string TokenKey = "spix_token";

    private string? _token;

    public string UserName { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public async Task<string?> GetTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_token))
        {
            return _token;
        }

        _token = await SecureStorage.Default.GetAsync(TokenKey);

        if (!string.IsNullOrWhiteSpace(_token))
        {
            LeerClaims(_token);
        }

        return _token;
    }

    public async Task SetTokenAsync(string token)
    {
        _token = token;
        LeerClaims(token);

        await SecureStorage.Default.SetAsync(TokenKey, token);
    }

    public void Clear()
    {
        _token = null;
        UserName = string.Empty;
        FullName = string.Empty;

        SecureStorage.Default.Remove(TokenKey);
    }

    //Hay sesion si el token existe y todavia no vencio
    public async Task<bool> IsLoggedAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            return jwt.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            //Token corrupto: se trata como si no hubiera sesion
            return false;
        }
    }

    private void LeerClaims(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            UserName = jwt.Claims.FirstOrDefault(x => x.Type == "UserName")?.Value
                       ?? jwt.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value
                       ?? string.Empty;

            FullName = jwt.Claims.FirstOrDefault(x => x.Type == "FirstName")?.Value is { } nombre
                ? $"{nombre} {jwt.Claims.FirstOrDefault(x => x.Type == "LastName")?.Value}".Trim()
                : UserName;
        }
        catch
        {
            UserName = string.Empty;
            FullName = string.Empty;
        }
    }
}
