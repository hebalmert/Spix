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
    private readonly ILocalStorageService _storage; private readonly HttpClient _http; private readonly ISessionServiceModel<SessionModelDTO> _session; private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity())); private string? _token;
    public AuthenticationProviderJWT(ILocalStorageService storage, HttpClient http, ISessionServiceModel<SessionModelDTO> session) { _storage = storage; _http = http; _session = session; }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync() { await _storage.RemoveItemAsync("TOKEN_KEY"); if (!Valid(_token)) await RefreshAsync(); return Valid(_token) ? State(_token!) : _anonymous; }
    public Task<string?> GetAccessTokenAsync() => Task.FromResult(Valid(_token) ? _token : null);
    public Task LoginAsync(string token) { _token = token; NotifyAuthenticationStateChanged(Task.FromResult(State(token))); return Task.CompletedTask; }
    public async Task LogoutAsync() { try { await _http.PostAsync("api/v1/accounts/Logout", null); } catch { } _token = null; await _session.ClearSessionAsync("SessionDTO"); await _storage.RemoveItemAsync("lastActivity"); _http.DefaultRequestHeaders.Authorization = null; NotifyAuthenticationStateChanged(Task.FromResult(_anonymous)); }
    private async Task RefreshAsync() { try { var response = await _http.PostAsync("api/v1/accounts/RefreshToken", null); var value = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TokenDTO>() : null; if (value is not null && Valid(value.Token)) _token = value.Token; } catch { } }
    private AuthenticationState State(string token) { _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new JwtSecurityTokenHandler().ReadJwtToken(token).Claims, "jwt"))); }
    private static bool Valid(string? token) { try { return !string.IsNullOrWhiteSpace(token) && new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo > DateTime.UtcNow; } catch { return false; } }
}
