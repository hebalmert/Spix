using Blazored.LocalStorage;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Localization;
using Spix.AppFront;
using Spix.AppFront.AuthenticationProviders;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.Helpers.Security;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.ResponcesSec;
using Spix.HttpService;
using Spix.xLanguage.Resources;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Modelo hosted: el front se sirve desde el mismo sitio que el API → mismo origen (dev y prod).
builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Almacenamiento Local para persistir data
builder.Services.AddBlazoredLocalStorage();

// Registrar HttpResponseHandler
builder.Services.AddScoped<HttpResponseHandler>();

// Registrar IRepository como Repository
builder.Services.AddScoped<IRepository>(sp => sp.GetRequiredService<Repository>());

//Para encriptar los datos que se guardaran en LocalStorage
var cryptoConfig = new CryptoSetting(); // Ya tiene valores por defecto
builder.Services.AddSingleton(cryptoConfig);
builder.Services.AddScoped<ICryptoService>(sp =>
{
    var config = sp.GetRequiredService<CryptoSetting>();
    return new CryptoService(config);
});

// Reemplazar la configuraciÃ³n del IRepository
builder.Services.AddScoped(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var authenticationProvider = sp.GetRequiredService<AuthenticationProviderJWT>();

    return new Repository(
        httpClient,
        async () =>
        {
            return await authenticationProvider.GetAccessTokenAsync();
        }
    );
});

//Para sistema de multilenguaje
builder.Services.AddLocalization();
builder.Services.AddSingleton<IStringLocalizer>(sp => sp.GetRequiredService<IStringLocalizer<Resource>>());
builder.Services.AddScoped<IEnumMultilLanguageService, EnumMultilLanguageService>();

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


