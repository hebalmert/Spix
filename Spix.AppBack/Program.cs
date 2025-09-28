using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.Domain.Resources;
using System.Globalization;
using System.Text.Json.Serialization;
using AppUser = Spix.Domain.Entities.User;
using Spix.DomainLogic.ResponcesSec;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Spix.AppBack.Data;
using Spix.AppBack.LoadCountries;
using Spix.AppBack.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

try
{
    //Localización
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.AddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();
    builder.Services.AddSingleton(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
    builder.Services.AddSingleton<IStringLocalizer>(sp =>
    {
        var factory = sp.GetRequiredService<IStringLocalizerFactory>();
        return factory.Create("Resource", typeof(Resource).Assembly.GetName().Name!);
    });

    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var cultures = new[] { "es", "en" }.Select(c => new CultureInfo(c)).ToList();
        options.DefaultRequestCulture = new RequestCulture("en");
        options.SupportedCultures = cultures;
        options.SupportedUICultures = cultures;
        options.RequestCultureProviders = new List<IRequestCultureProvider>
        {
            new QueryStringRequestCultureProvider(),
            new AcceptLanguageHeaderRequestCultureProvider(),
            new CookieRequestCultureProvider()
        };
    });

    //Base de datos
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está definida.");

    builder.Services.AddDbContext<DataContext>(x =>
        x.UseSqlServer(connectionString, option => option.MigrationsAssembly("Spix.AppBack")));

    //Identity
    builder.Services.AddIdentity<AppUser, IdentityRole>(cfg =>
    {
        cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
        cfg.SignIn.RequireConfirmedEmail = true;
        cfg.User.RequireUniqueEmail = false;
        cfg.Password.RequireDigit = false;
        cfg.Password.RequiredUniqueChars = 0;
        cfg.Password.RequireLowercase = false;
        cfg.Password.RequireNonAlphanumeric = false;
        cfg.Password.RequireUppercase = false;
        cfg.Lockout.MaxFailedAccessAttempts = 3;
        cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        cfg.Lockout.AllowedForNewUsers = true;
    }).AddDefaultTokenProviders()
      .AddEntityFrameworkStores<DataContext>();

    //JWT
    var jwtKey = builder.Configuration["jwtKey"];
    if (string.IsNullOrEmpty(jwtKey))
        throw new InvalidOperationException("'jwtKey' no está definido en la configuración.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddCookie()
        .AddJwtBearer(x => x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        });

    //Configuración de secciones
    builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGrid"));
    builder.Services.Configure<SendSmsSetting>(builder.Configuration.GetSection("SendSms"));
    builder.Services.Configure<ImgSetting>(builder.Configuration.GetSection("ImgSoftware"));
    builder.Services.Configure<AzureSetting>(builder.Configuration.GetSection("ConnectionStrings"));
    builder.Services.Configure<JwtKeySetting>(options => options.jwtKey = jwtKey);

    //Infraestructura
    builder.Services.AddControllers()
        .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    builder.Services.AddOpenApi();
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddMvc().AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddTransient<SeedDb>();
    builder.Services.AddScoped<IApiService, ApiService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMemoryCache();
    InfraRegistration.AddInfraRegistration(builder.Services, builder.Configuration);
    UnitOfWorkRegistration.AddUnitOfWorkRegistration(builder.Services);

    //CORS
    string? frontUrl = builder.Configuration["UrlFrontend"];
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin", builder =>
        {
            builder.WithOrigins(frontUrl!, "https://megaxappfront-g9hrfda7gqfxcce6.canadacentral-01.azurewebsites.net")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .WithExposedHeaders(new[] { "Totalpages", "Counting" });
        });
    });

    //Construcción y arranque
    var app = builder.Build();

    var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
    app.UseRequestLocalization(localizationOptions);

    //Seeder con manejo de errores
    try
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SeedDb>();
        await seeder.SeedAsync();
        Console.WriteLine("Seeder ejecutado correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en Seeder: {ex.Message}");
    }

    app.UseCors("AllowSpecificOrigin");
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Console.WriteLine(" App corriendo...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($" Error crítico en Program.cs: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;
}