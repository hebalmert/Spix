using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Spix.AppFront.Helper;
using Spix.DomainLogic.AppResponses;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Spix.AppFront.AuthenticationProviders
{
    public class AuthenticationProviderJWT : AuthenticationStateProvider, ILoginService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;
        private readonly ISessionServiceModel<SessionModelDTO> _sessionModel;
        private const string TokenKey = "TOKEN_KEY";
        private readonly AuthenticationState _anonimous;

        public AuthenticationProviderJWT(ILocalStorageService localStorage, HttpClient httpClient,
            ISessionServiceModel<SessionModelDTO> SessionModel)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
            _sessionModel = SessionModel;
            _anonimous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            if (string.IsNullOrWhiteSpace(token))
                return _anonimous;

            return BuildAuthenticationState(token);
        }

        public async Task LoginAsync(string token)
        {
            await _localStorage.SetItemAsync(TokenKey, token);
            var authState = BuildAuthenticationState(token);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task LogoutAsync()
        {
            await _sessionModel.ClearSessionAsync("SessionDTO");
            await _localStorage.RemoveItemAsync("lastActivity");
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.ClearAsync();
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(Task.FromResult(_anonimous));
        }

        private AuthenticationState BuildAuthenticationState(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var claims = ParseClaimsFromJWT(token);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }

        private IEnumerable<Claim> ParseClaimsFromJWT(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims;
        }
    }
}