using Mapster;
using Spix.AppInfra.Caching;
using Spix.AppInfra.Configuration;
using MapsterMapper;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.SecretProtection;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.UtilityTools;
using Spix.DomainLogic.Configuration;
using Spix.xFiles.ExcelHelper;
using Spix.xFiles.FileHelper;
using Spix.xFiles.QRgenerate;
using Spix.xFiles.SignatureHelper;
using Spix.xNetwork.IpHelper;
using Spix.xNetwork.MacHelper;
using Spix.xNetwork.PingHelper;
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

            // Cache de los combos de catalogos globales (paises, ciudades, tipos...)
            services.AddSingleton<IComboCache, ComboCache>();

            //Para generacion del QR para hacer visitas rapidas de Pacientes.
            services.AddSingleton<IQRService, QRService>();

            // Manejo de transacciones por request
            services.AddScoped<ITransactionManager, TransactionManager>();

            // Manejo de Ping para verificar la conectividad de red
            services.AddScoped<IPingControl, PingControl>();

            // Utilidades para manejo de Imagenes o Archivos xFiles
            services.AddScoped<IFileStorage, FileStorage>();
            services.AddScoped<IPdfSignatureService, PdfSignatureService>();
            services.AddScoped<IExcelParser, ExcelParser>();
            services.AddScoped<IExcelExporter, ExcelExporter>();

            // Manejo de Procesos para el Network
            services.AddScoped<IIpControl, IpControl>();
            services.AddScoped<IIpNetControl, IpNetControl>();

            //Marnejo de Mac
            services.AddScoped<IMacControl, MacControl>();

            // Utilidades para autenticación y gestión de usuarios
            services.AddScoped<IUserHelper, UserHelper>();

            // Herramientas generales sin estado
            services.AddTransient<IUtilityTools, UtilityTools>();

            // Multilenguaje para opciones de enums
            services.AddScoped<IEnumMultilLanguageService, EnumMultilLanguageService>();

            // Servicio de envío de correos
            services.AddScoped<ISecretProtector, AesSecretProtector>();
            services.AddScoped<ISecretStore, SecretStore>();
            services.AddTransient<IEmailDeliveryService, EmailDeliveryService>();
            services.AddTransient<IEmailHelper, EmailHelper>();

            // Configuración y mapeo con Mapster
            MapsterConfig.RegisterMappings();
            services.AddSingleton(TypeAdapterConfig.GlobalSettings);
            services.AddScoped<IMapper, ServiceMapper>();
            services.AddScoped<IMapperService, MapperService>();
        }
    }
}



