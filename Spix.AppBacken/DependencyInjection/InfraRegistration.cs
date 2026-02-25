using Mapster;
using MapsterMapper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.UtilityTools;
using Spix.xFiles.ExcelHelper;
using Spix.xFiles.FileHelper;
using Spix.xFiles.QRgenerate;
using Spix.xNotification.Implements;
using Spix.xNotification.Interfaces;

namespace Spix.AppBack.DependencyInjection
{
    public class InfraRegistration
    {
        public static void AddInfraRegistration(IServiceCollection services, IConfiguration config)
        {
            // Manejo de Errores
            services.AddScoped<HttpErrorHandler>();

            //Para generacion del QR para hacer visitas rapidas de Pacientes.
            services.AddSingleton<IQRService, QRService>();

            // Manejo de transacciones por request
            services.AddScoped<ITransactionManager, TransactionManager>();

            // Utilidades para manejo de Imagenes o Archivos xFiles
            services.AddScoped<IFileStorage, FileStorage>();
            services.AddScoped<IExcelParser, ExcelParser>();
            services.AddScoped<IExcelExporter, ExcelExporter>();

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
}