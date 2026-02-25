using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Spix.AppBack.Data;
using Spix.AppBack.DependencyInjection;
using Spix.AppBack.LoadCountries;
using Spix.AppInfra;
using Spix.DomainLogic.SettingModels;
using Spix.xLanguage.Resources;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using AppUser = Spix.Domain.Entities.User;

var builder = WebApplication.CreateBuilder(args);

//Localización para manejar sistemas de multilenguaje y centrar repuestas
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

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
})
.AddMvc(options =>
{
    options.Conventions.Add(new VersionByNamespaceConvention());
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger (solo Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Documentos por versión
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders Backend - V1", Version = "1.0" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "Orders Backend - V2", Version = "2.0" });

    // Filtrado por versión
    options.DocInclusionPredicate((version, desc) =>
    {
        var versions = desc.ActionDescriptor.EndpointMetadata
            .OfType<ApiVersionAttribute>()
            .SelectMany(attr => attr.Versions);

        return versions.Any(v => $"v{v.MajorVersion}" == version);
    });

    // Evitar nombres duplicados
    options.CustomSchemaIds(type => type.Name.Replace("Controller", "").Replace("V", ""));

    // JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. <br /> <br />
                      Enter 'Bearer' [space] and then your token in the text input below.<br /> <br />
                      Example: 'Bearer 12345abcdef'<br /> <br />",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,
              },
            new List<string>()
          }
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["jwtKey"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("'jwtKey' no está definido en la configuración.");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

//Conexion de la Base de Datos SQL Server usando Entity Framework Core.  Aca Podemos cambiar el tipo de Conexion a otra BD
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está definida.");

//Inyectamos el Contexto desde AppInfra y establecemos AppBack como el proyecto de migraciones y Update-Database
builder.Services.AddDbContext<DataContext>(x =>
    x.UseSqlServer(connectionString, option => option.MigrationsAssembly("Spix.AppBacken")));

//Identity Como vamos a menajar los usuarios y roles dentro del sistema, las validaciones de los mismos
builder.Services.AddIdentity<AppUser, IdentityRole>(cfg =>
{
    cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    cfg.SignIn.RequireConfirmedEmail = false;
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

//Configuraciones de sistemas IOption para inyectar en Proyectos fuera de AppBack dentro del Solution Aban.
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGrid"));
builder.Services.Configure<ImgSetting>(builder.Configuration.GetSection("ImgSoftware"));
builder.Services.Configure<AzureSetting>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<JwtKeySetting>(options => options.jwtKey = jwtKey);

//Ejecucion de serivicios especiales cada vez que hagamos un Pusho del App ha hosting, forma de cargar datos iniciales
builder.Services.AddTransient<SeedDb>();

//Creacion de un servicio para llenado de Pais, Estado, Ciudad desde un API de terceros
builder.Services.AddScoped<IApiService, ApiService>();

//Servicio para pase de HttpContext y poder manejar las respuestas de Multilenguaje sin problemas el los servicios
builder.Services.AddHttpContextAccessor();

//Manejo de Cache en memoria para optimizar respuestas y cargas de datos frecuentes se puede usar en Clases Abtractas.
builder.Services.AddMemoryCache();

//Clases donde vamos a tener la implementacion de Services de UnitOfWork y Services ademas de Servicios Inyectables especiales.
//Esto se hace para evitar saturar el Program.cs y mantener un codigo mas ordenado
InfraRegistration.AddInfraRegistration(builder.Services, builder.Configuration);
UnitOfWorkRegistration.AddUnitOfWorkRegistration(builder.Services);

//CORS permitir solicitudes desde el Frontend y establecemos la URL que podra acceder a este Backend
//"Totalpages", "Counting"   manjamos por los Headers la paginacion, de esa manera no tenemos que hacer consultas alternas para obtener esos datos
string? frontUrl = builder.Configuration["UrlFrontend"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("https://localhost:7137", "https://regixappfront-cngmebf8gsbyehd9.canadacentral-01.azurewebsites.net")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .WithExposedHeaders(new[] { "Totalpages", "Counting" });
    });
});

var app = builder.Build();
//Aplicando el sistema de Localizacion para Multilenguaje
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

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders Backend - V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Orders Backend - V2");
    });
}

app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

app.Run();
