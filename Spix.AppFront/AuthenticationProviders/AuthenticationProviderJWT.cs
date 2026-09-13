using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Spix.AppFront.Helper;
using Spix.DomainLogic.AppResponses;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
namespace Spix.AppFront.AuthenticationProviders;
public class AuthenticationProviderJWT : AuthenticationStateProvider, ILoginService
{
    private readonly ILocalStorageService _storage; private readonly HttpClient _http; private readonly ISessionServiceModel<SessionModelDTO> _session; private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity())); private string? _token; private Task? _refreshInFlight;
    public AuthenticationProviderJWT(ILocalStorageService storage, HttpClient http, ISessionServiceModel<SessionModelDTO> session) { _storage = storage; _http = http; _session = session; }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync() { await _storage.RemoveItemAsync("TOKEN_KEY"); if (!Valid(_token)) await RefreshOnceAsync(); return Valid(_token) ? State(_token!) : _anonymous; }
    //Si el access token vencio (o vence en menos de 30 s) se renueva con la cookie ANTES de llamar al API.
    //Sin esto la peticion salia sin token, el API respondia 401 y se cerraba la sesion en pleno uso.
    public async Task<string?> GetAccessTokenAsync()
    {
        //Solo si HABIA sesion: sin token (usuario anonimo, landing, login) no se llama a RefreshToken,
        //igual que antes, para no sumar una peticion extra a cada llamada publica.
        if (_token is not null && !Valid(_token, TimeSpan.FromSeconds(30))) await RefreshOnceAsync();

        return Valid(_token) ? _token : null;
    }
    public async Task LoginAsync(string token)
    {
        _token = token;

        //Un login nuevo arranca con el reloj de inactividad en cero. Sin esto, IdleLogout leia el
        //"lastActivity" que dejo la sesion ANTERIOR (por ejemplo, si se cerro la pestana sin salir),
        //creia que el usuario llevaba mas del limite inactivo y cerraba la sesion recien abierta:
        //por eso a veces habia que loguearse dos veces.
        await _storage.SetItemAsync("lastActivity", DateTime.UtcNow.ToString("o"));

        NotifyAuthenticationStateChanged(Task.FromResult(State(token)));
    }
    public async Task LogoutAsync() { try { await _http.PostAsync("api/v1/accounts/Logout", null); } catch { } _token = null; await _session.ClearSessionAsync("SessionDTO"); await _storage.RemoveItemAsync("lastActivity"); _http.DefaultRequestHeaders.Authorization = null; NotifyAuthenticationStateChanged(Task.FromResult(_anonymous)); }
    private async Task RefreshAsync() { try { var response = await _http.PostAsync("api/v1/accounts/RefreshToken", null); var value = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TokenDTO>() : null; if (value is not null && Valid(value.Token)) _token = value.Token; } catch { } }
    //Varios componentes piden el estado de sesion al mismo tiempo (Router, layout, IdleLogout...).
    //Con la rotacion del refresh token, si cada uno llamaba a RefreshToken por su cuenta, la primera
    //llamada invalidaba el token y las demas llegaban con el viejo y fallaban, dejando al usuario como
    //anonimo. Todos esperan ahora la MISMA renovacion.
    private async Task RefreshOnceAsync()
    {
        _refreshInFlight ??= RefreshAsync();

        try
        {
            await _refreshInFlight;
        }
        finally
        {
            _refreshInFlight = null;
        }
    }

    private AuthenticationState State(string token) { _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new JwtSecurityTokenHandler().ReadJwtToken(token).Claims, "jwt"))); }
    private static bool Valid(string? token, TimeSpan margin = default) { try { return !string.IsNullOrWhiteSpace(token) && new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo > DateTime.UtcNow.Add(margin); } catch { return false; } }
}
