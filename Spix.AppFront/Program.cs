using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Spix.AppFront;
using Spix.AppFront.Helpers.Security;
using Spix.AppFront.AuthenticationProviders;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.DomainLogic.ResponcesSec;
using Blazored.LocalStorage;
using Spix.HttpService;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7032") });

// Almacenamiento Local para persistir data
builder.Services.AddBlazoredLocalStorage();

//Para encriptar los datos que se guardaran en LocalStorage
var cryptoConfig = new CryptoSetting(); // Ya tiene valores por defecto
builder.Services.AddSingleton(cryptoConfig);
builder.Services.AddScoped<ICryptoService>(sp =>
{
    var config = sp.GetRequiredService<CryptoSetting>();
    return new CryptoService(config);
});

// Registrar HttpResponseHandler
builder.Services.AddScoped<HttpResponseHandler>();

// Registrar IRepository como Repository
builder.Services.AddScoped<IRepository>(sp => sp.GetRequiredService<Repository>());

// Para Recuperar el Token del Storage y agregarlo a las peticiones Http
builder.Services.AddScoped(sp =>
{
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var httpClient = sp.GetRequiredService<HttpClient>();

    return new Repository(
        httpClient,
        async () =>
        {
            var token = await localStorage.GetItemAsync<string>("TOKEN_KEY");
            return string.IsNullOrWhiteSpace(token) ? null : token;
        }
    );
});

//Para sistema de multilenguaje
builder.Services.AddLocalization();

// Sistema de Seguridad
builder.Services.AddAuthorizationCore();

// Manejar el SweetAlert de mensajes
builder.Services.AddSweetAlert2();

//Para manejar los Modales como MudBlazor
builder.Services.AddScoped<ModalService>();

//Para manejar valor de session por la aplicacion
builder.Services.AddScoped<ISessionServiceModel<SessionModelDTO>, SessionServiceModel<SessionModelDTO>>();

//Authentication Provider
builder.Services.AddScoped<AuthenticationProviderJWT>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationProviderJWT>(x => x.GetRequiredService<AuthenticationProviderJWT>());
builder.Services.AddScoped<ILoginService, AuthenticationProviderJWT>(x => x.GetRequiredService<AuthenticationProviderJWT>());

await builder.Build().RunAsync();