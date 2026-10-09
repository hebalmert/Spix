using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Spix.AppMaui.Configuration;
using Spix.AppMaui.Services;
using Spix.AppMaui.ViewModels;
using Spix.AppMaui.Views;
using Spix.HttpService;
using System.Text.Json;

namespace Spix.AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        //La direccion del Backend sale de appsettings.json, igual que en el escritorio
        var apiSettings = LeerApiSettings();

        if (!Uri.TryCreate(apiSettings.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("La URL configurada para el Backend no es valida.");
        }

        builder.Services.AddSingleton(apiSettings);
        builder.Services.AddSingleton(new HttpClient { BaseAddress = baseUri });

        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<AlertService>();
        builder.Services.AddSingleton<LocationService>();
        builder.Services.AddSingleton<PhotoService>();
        builder.Services.AddSingleton<ApiResponseHandler>();

        //El mismo cliente HTTP de la web y del escritorio: pone el Bearer solo
        builder.Services.AddSingleton<IRepository>(sp => new Repository(
            sp.GetRequiredService<HttpClient>(),
            sp.GetRequiredService<SessionService>().GetTokenAsync));

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<TodayViewModel>();
        builder.Services.AddTransient<UpcomingViewModel>();
        builder.Services.AddTransient<ClosedViewModel>();
        builder.Services.AddTransient<VisitDetailViewModel>();
        builder.Services.AddTransient<NoClientViewModel>();
        builder.Services.AddTransient<AddServiceViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<TodayPage>();
        builder.Services.AddTransient<UpcomingPage>();
        builder.Services.AddTransient<ClosedPage>();
        builder.Services.AddTransient<VisitDetailPage>();
        builder.Services.AddTransient<NoClientPage>();
        builder.Services.AddTransient<AddServicePage>();
        builder.Services.AddTransient<ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    //El archivo va empaquetado como MauiAsset: se lee del paquete, no del disco
    private static ApiSettings LeerApiSettings()
    {
        using var stream = FileSystem.OpenAppPackageFileAsync(ApiSettings.FileName)
            .GetAwaiter().GetResult();

        using var lector = new StreamReader(stream);
        var json = lector.ReadToEnd();

        using var documento = JsonDocument.Parse(json);

        var baseUrl = documento.RootElement
            .GetProperty("ApiSettings")
            .GetProperty("BaseUrl")
            .GetString();

        return new ApiSettings { BaseUrl = baseUrl ?? string.Empty };
    }
}
