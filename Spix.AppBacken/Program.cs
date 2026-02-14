using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Spix.xLanguage.Resources;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

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
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
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

var app = builder.Build();
//Aplicando el sistema de Localizacion para Multilenguaje
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

app.Run();
