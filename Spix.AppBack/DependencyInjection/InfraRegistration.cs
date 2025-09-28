using Mapster;
using MapsterMapper;
using Spix.AppInfra.EmailHelper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.FileHelper;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.UtilityTools;

namespace Spix.AppBack.DependencyInjection;

public class InfraRegistration
{
    public static void AddInfraRegistration(IServiceCollection services, IConfiguration config)
    {
        // Manejo de Errores
        services.AddScoped<HttpErrorHandler>();

        // Manejo de transacciones por request
        services.AddScoped<ITransactionManager, TransactionManager>();

        // Utilidades para manejo de Imagenes o Archivos
        services.AddScoped<IFileStorage, FileStorage>();

        // Utilidades para autenticación y gestión de usuarios
        services.AddScoped<IUserHelper, UserHelper>();

        // Herramientas generales sin estado
        services.AddTransient<IUtilityTools, UtilityTools>();

        // Servicio de envío de correos
        services.AddTransient<IEmailHelper, EmailHelper>();

        // Configuración y mapeo con Mapster
        MapsterConfig.RegisterMappings();
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddScoped<IMapper, ServiceMapper>();
        services.AddScoped<IMapperService, MapperService>();
    }
}