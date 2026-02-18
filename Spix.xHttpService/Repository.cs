using Microsoft.IdentityModel.Tokens;
using Spix.xFiles.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Spix.HttpService;

public class Repository : IRepository
{
    private readonly HttpClient _httpClient;
    private readonly Func<Task<string?>> _getToken;

    private JsonSerializerOptions _jsonOptions => new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Repository(HttpClient httpClient, Func<Task<string?>> getToken)
    {
        _httpClient = httpClient;
        _getToken = getToken;
    }

    private async Task AddAuthorizationHeader()
    {
        var token = await _getToken();

        if (string.IsNullOrWhiteSpace(token))
            return; // No hay token, no se agrega el header

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt;

        try
        {
            jwt = handler.ReadJwtToken(token);
        }
        catch
        {
            // Token mal formado o inválido
            throw new SecurityTokenException("Token inválido o corrupto.");
        }

        if (jwt.ValidTo < DateTime.UtcNow)
            throw new SecurityTokenExpiredException("Token expirado.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<HttpResponseWrapper<DownloadFileDTO>> GetDownloadUrlAsync(string container, string file, string nameOriginal)
    {
        await AddAuthorizationHeader();
        var url = $"api/v1/files/url-descarga/{container}/{file}/{nameOriginal}";
        var responseHttp = await _httpClient.GetAsync(url);

        if (responseHttp.IsSuccessStatusCode)
        {
            var json = await responseHttp.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<DownloadFileDTO>(json, _jsonOptions);
            return new HttpResponseWrapper<DownloadFileDTO>(dto!, false, responseHttp);
        }

        return new HttpResponseWrapper<DownloadFileDTO>(default!, true, responseHttp);
    }

    public async Task<HttpResponseWrapper<byte[]>> GetFileAsync(string url)
    {
        await AddAuthorizationHeader();
        var responseHttp = await _httpClient.GetAsync(url);

        if (responseHttp.IsSuccessStatusCode)
        {
            var fileBytes = await responseHttp.Content.ReadAsByteArrayAsync();
            return new HttpResponseWrapper<byte[]>(fileBytes, false, responseHttp);
        }

        return new HttpResponseWrapper<byte[]>(Array.Empty<byte>(), true, responseHttp);
    }

    public async Task<HttpResponseWrapper<object>> GetAsync(string url)
    {
        await AddAuthorizationHeader();
        var responseHTTP = await _httpClient.GetAsync(url);
        return new HttpResponseWrapper<object>(null, !responseHTTP.IsSuccessStatusCode, responseHTTP);
    }

    public async Task<HttpResponseWrapper<T>> GetAsync<T>(string url)
    {
        await AddAuthorizationHeader();
        var responseHttp = await _httpClient.GetAsync(url);
        if (responseHttp.IsSuccessStatusCode)
        {
            var response = await UnserializeAnswerAsync<T>(responseHttp);
            return new HttpResponseWrapper<T>(response, false, responseHttp);
        }

        return new HttpResponseWrapper<T>(default, true, responseHttp);
    }

    public async Task<HttpResponseWrapper<object>> PostAsync<T>(string url, T model)
    {
        await AddAuthorizationHeader();
        var messageJSON = JsonSerializer.Serialize(model);
        var messageContet = new StringContent(messageJSON, Encoding.UTF8, "application/json");
        var responseHttp = await _httpClient.PostAsync(url, messageContet);
        return new HttpResponseWrapper<object>(null, !responseHttp.IsSuccessStatusCode, responseHttp);
    }

    public async Task<HttpResponseWrapper<TResponse>> PostAsync<T, TResponse>(string url, T model)
    {
        await AddAuthorizationHeader();
        var messageJSON = JsonSerializer.Serialize(model);
        var messageContet = new StringContent(messageJSON, Encoding.UTF8, "application/json");
        var responseHttp = await _httpClient.PostAsync(url, messageContet);
        if (responseHttp.IsSuccessStatusCode)
        {
            var response = await UnserializeAnswerAsync<TResponse>(responseHttp);
            return new HttpResponseWrapper<TResponse>(response, false, responseHttp);
        }

        return new HttpResponseWrapper<TResponse>(default, !responseHttp.IsSuccessStatusCode, responseHttp);
    }

    public async Task<HttpResponseWrapper<object>> PutAsync<T>(string url, T model)
    {
        await AddAuthorizationHeader();
        var messageJson = JsonSerializer.Serialize(model);
        var messageContent = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var responseHttp = await _httpClient.PutAsync(url, messageContent);
        return new HttpResponseWrapper<object>(null, !responseHttp.IsSuccessStatusCode, responseHttp);
    }

    public async Task<HttpResponseWrapper<TResponse>> PutAsync<T, TResponse>(string url, T model)
    {
        await AddAuthorizationHeader();
        var messageJson = JsonSerializer.Serialize(model);
        var messageContent = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var responseHttp = await _httpClient.PutAsync(url, messageContent);
        if (responseHttp.IsSuccessStatusCode)
        {
            var response = await UnserializeAnswerAsync<TResponse>(responseHttp);
            return new HttpResponseWrapper<TResponse>(response, false, responseHttp);
        }

        return new HttpResponseWrapper<TResponse>(default, true, responseHttp);
    }

    public async Task<HttpResponseWrapper<object>> DeleteAsync(string url)
    {
        await AddAuthorizationHeader();
        var responseHttp = await _httpClient.DeleteAsync(url);
        return new HttpResponseWrapper<object>(null, !responseHttp.IsSuccessStatusCode, responseHttp);
    }

    private async Task<T> UnserializeAnswerAsync<T>(HttpResponseMessage responseHttp)
    {
        var response = await responseHttp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(response, _jsonOptions)!;
    }
}