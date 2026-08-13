using FontAwesome.Net.Generators;
using FontAwesome.Net.Wpf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spix.AppWpf.Configuration;
using Spix.AppWpf.Services.Auth;
using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Services.Session;
using Spix.AppWpf.ViewModels.Auth;
using Spix.AppWpf.ViewModels.EntitiesGen.DocumentType;
using Spix.AppWpf.ViewModels.EntitiesGen.EstratoSocial;
using Spix.AppWpf.ViewModels.EntitiesGen.Plan;
using Spix.AppWpf.ViewModels.EntitiesGen.Service;
using Spix.AppWpf.ViewModels.EntitiesInven.Product;
using Spix.AppWpf.ViewModels.EntitiesInven.Mark;
using Spix.AppWpf.ViewModels.EntitiesInven.Supplier;
using Spix.AppWpf.ViewModels.EntitiesInven.Storage;
using Spix.AppWpf.ViewModels.EntitiesInven.Purchase;
using Spix.AppWpf.ViewModels.EntitiesInven.Serial;
using Spix.AppWpf.ViewModels.EntitiesInven.Cargue;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNet;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNetwork;
using Spix.AppWpf.ViewModels.EntitiesNet.Node;
using Spix.AppWpf.ViewModels.EntitiesMK.ConnectionMikrotikControl;
using Spix.AppWpf.ViewModels.EntitiesMK.QueueType;
using Spix.AppWpf.ViewModels.Shell;
using Spix.AppWpf.Views.Auth;
using Spix.AppWpf.Views.EntitiesGen.DocumentType;
using Spix.AppWpf.Views.EntitiesGen.EstratoSocial;
using Spix.AppWpf.Views.EntitiesGen.Plan;
using Spix.AppWpf.Views.EntitiesGen.Service;
using Spix.AppWpf.Views.EntitiesInven.Product;
using Spix.AppWpf.Views.EntitiesInven.Mark;
using Spix.AppWpf.Views.EntitiesInven.Supplier;
using Spix.AppWpf.Views.EntitiesInven.Storage;
using Spix.AppWpf.Views.EntitiesInven.Purchase;
using Spix.AppWpf.Views.EntitiesInven.Serial;
using Spix.AppWpf.Views.EntitiesInven.Cargue;
using Spix.AppWpf.Views.EntitiesNet.IpNet;
using Spix.AppWpf.Views.EntitiesNet.IpNetwork;
using Spix.AppWpf.Views.EntitiesNet.Node;
using Spix.AppWpf.Views.EntitiesMK.ConnectionMikrotikControl;
using Spix.AppWpf.Views.EntitiesMK.QueueType;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.Services.Network;
using Spix.AppWpf.NetHelper;
using Spix.HttpService;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;

namespace Spix.AppWpf;

// Representa el punto de entrada de la aplicación WPF.
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    // Registra las fuentes antes de crear la ventana inicial de la aplicación.
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                optional: false,
                reloadOnChange: false)
            .Build();

        var apiSettings = new ApiSettings
        {
            BaseUrl = configuration[$"{ApiSettings.SectionName}:BaseUrl"] ?? string.Empty
        };

        if (!Uri.TryCreate(apiSettings.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException(
                "La URL configurada para el Backend no es valida.");
        }

        var services = new ServiceCollection();

        // Registra objetos compartidos para todas las vistas y servicios desktop.
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(apiSettings);
        services.AddSingleton(new HttpClient
        {
            BaseAddress = baseUri
        });
        services.AddSingleton<IUserSessionService, UserSessionService>();
        services.AddSingleton<IRepository>(serviceProvider => new Repository(
            serviceProvider.GetRequiredService<HttpClient>(),
            serviceProvider.GetRequiredService<IUserSessionService>().GetTokenAsync));
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IAccountService, AccountService>();
        services.AddSingleton(typeof(IPagedEntityService<>), typeof(PagedEntityService<>));
        services.AddSingleton<ModalService>();
        services.AddSingleton<AlertService>();
        services.AddSingleton<HttpResponseHandler>();
        // Ejecuta ping y consultas MikroTik desde la red local del equipo Windows.
        services.AddSingleton<IPingControl, PingControl>();
        services.AddSingleton<IMkConnectionControl, MkConnectionControl>();
        // Ejecuta MikroTik desde la LAN o Wi-Fi del Windows, no desde la red del Backend.
        services.AddSingleton<ILocalMikrotikService, LocalMikrotikService>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ChangePasswordViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DocumentTypeIndexViewModel>();
        services.AddTransient<CreateDocumentTypeDialogViewModel>();
        services.AddTransient<EditDocumentTypeDialogViewModel>();
        services.AddTransient<EstratoSocialIndexViewModel>();
        services.AddTransient<CreateEstratoSocialDialogViewModel>();
        services.AddTransient<EditEstratoSocialDialogViewModel>();
        services.AddTransient<PlanIndexViewModel>();
        services.AddTransient<CreatePlanCategoryDialogViewModel>();
        services.AddTransient<EditPlanCategoryDialogViewModel>();
        services.AddTransient<CreatePlanDialogViewModel>();
        services.AddTransient<EditPlanDialogViewModel>();
        services.AddTransient<ServiceIndexViewModel>();
        services.AddTransient<CreateServiceCategoryDialogViewModel>();
        services.AddTransient<EditServiceCategoryDialogViewModel>();
        services.AddTransient<CreateServiceClientDialogViewModel>();
        services.AddTransient<EditServiceClientDialogViewModel>();
        services.AddTransient<ProductIndexViewModel>();
        services.AddTransient<CreateProductCategoryDialogViewModel>();
        services.AddTransient<EditProductCategoryDialogViewModel>();
        services.AddTransient<CreateProductDialogViewModel>();
        services.AddTransient<EditProductDialogViewModel>();
        services.AddTransient<MarkIndexViewModel>();
        services.AddTransient<CreateMarkDialogViewModel>();
        services.AddTransient<EditMarkDialogViewModel>();
        services.AddTransient<CreateMarkModelDialogViewModel>();
        services.AddTransient<EditMarkModelDialogViewModel>();
        services.AddTransient<SupplierIndexViewModel>();
        services.AddTransient<CreateSupplierDialogViewModel>();
        services.AddTransient<EditSupplierDialogViewModel>();
        services.AddTransient<ProductStorageIndexViewModel>();
        services.AddTransient<CreateProductStorageDialogViewModel>();
        services.AddTransient<EditProductStorageDialogViewModel>();
        services.AddTransient<PurchaseIndexViewModel>();
        services.AddTransient<PurchaseDetailsViewModel>();
        services.AddTransient<CreatePurchaseDialogViewModel>();
        services.AddTransient<EditPurchaseDialogViewModel>();
        services.AddTransient<CreatePurchaseDetailDialogViewModel>();
        services.AddTransient<EditPurchaseDetailDialogViewModel>();
        services.AddTransient<SerialIndexViewModel>();
        services.AddTransient<EditSerialDialogViewModel>();
        services.AddTransient<CargueIndexViewModel>();
        services.AddTransient<CargueDetailsViewModel>();
        services.AddTransient<CreateCargueDetailDialogViewModel>();
        services.AddTransient<EditCargueDetailDialogViewModel>();
        services.AddTransient<IpNetIndexViewModel>();
        services.AddTransient<CreateIpNetDialogViewModel>();
        services.AddTransient<EditIpNetDialogViewModel>();
        services.AddTransient<CreateIpNetPoolDialogViewModel>();
        services.AddTransient<DeleteIpNetPoolDialogViewModel>();
        services.AddTransient<IpNetworkIndexViewModel>();
        services.AddTransient<CreateIpNetworkDialogViewModel>();
        services.AddTransient<EditIpNetworkDialogViewModel>();
        services.AddTransient<NodeIndexViewModel>();
        services.AddTransient<CreateNodeDialogViewModel>();
        services.AddTransient<EditNodeDialogViewModel>();
        services.AddTransient<NodeMapDialogViewModel>();
        services.AddTransient<NodePingDialogViewModel>();
        services.AddTransient<CreateIpNetworkPoolDialogViewModel>();
        services.AddTransient<DeleteIpNetworkPoolDialogViewModel>();
        services.AddTransient<ConnectionMikrotikControlIndexViewModel>();
        services.AddTransient<CreateConnectionMikrotikControlDialogViewModel>();
        services.AddTransient<EditConnectionMikrotikControlDialogViewModel>();
        services.AddTransient<QueueTypeIndexViewModel>();
        services.AddTransient<CreateQueueTypeDialogViewModel>();
        services.AddTransient<EditQueueTypeDialogViewModel>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<ChangePasswordWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<DocumentTypeIndexView>();
        services.AddTransient<CreateDocumentTypeDialogView>();
        services.AddTransient<EditDocumentTypeDialogView>();
        services.AddTransient<EstratoSocialIndexView>();
        services.AddTransient<CreateEstratoSocialDialogView>();
        services.AddTransient<EditEstratoSocialDialogView>();
        services.AddTransient<PlanIndexView>();
        services.AddTransient<CreatePlanCategoryDialogView>();
        services.AddTransient<EditPlanCategoryDialogView>();
        services.AddTransient<CreatePlanDialogView>();
        services.AddTransient<EditPlanDialogView>();
        services.AddTransient<ServiceIndexView>();
        services.AddTransient<CreateServiceCategoryDialogView>();
        services.AddTransient<EditServiceCategoryDialogView>();
        services.AddTransient<CreateServiceClientDialogView>();
        services.AddTransient<EditServiceClientDialogView>();
        services.AddTransient<ProductIndexView>();
        services.AddTransient<CreateProductCategoryDialogView>();
        services.AddTransient<EditProductCategoryDialogView>();
        services.AddTransient<CreateProductDialogView>();
        services.AddTransient<EditProductDialogView>();
        services.AddTransient<MarkIndexView>();
        services.AddTransient<CreateMarkDialogView>();
        services.AddTransient<EditMarkDialogView>();
        services.AddTransient<CreateMarkModelDialogView>();
        services.AddTransient<EditMarkModelDialogView>();
        services.AddTransient<SupplierIndexView>();
        services.AddTransient<CreateSupplierDialogView>();
        services.AddTransient<EditSupplierDialogView>();
        services.AddTransient<ProductStorageIndexView>();
        services.AddTransient<CreateProductStorageDialogView>();
        services.AddTransient<EditProductStorageDialogView>();
        services.AddTransient<PurchaseIndexView>();
        services.AddTransient<PurchaseDetailsView>();
        services.AddTransient<CreatePurchaseDialogView>();
        services.AddTransient<EditPurchaseDialogView>();
        services.AddTransient<CreatePurchaseDetailDialogView>();
        services.AddTransient<EditPurchaseDetailDialogView>();
        services.AddTransient<SerialIndexView>();
        services.AddTransient<EditSerialDialogView>();
        services.AddTransient<CargueIndexView>();
        services.AddTransient<CargueDetailsView>();
        services.AddTransient<CreateCargueDetailDialogView>();
        services.AddTransient<EditCargueDetailDialogView>();
        services.AddTransient<IpNetIndexView>();
        services.AddTransient<CreateIpNetDialogView>();
        services.AddTransient<EditIpNetDialogView>();
        services.AddTransient<CreateIpNetPoolDialogView>();
        services.AddTransient<DeleteIpNetPoolDialogView>();
        services.AddTransient<IpNetworkIndexView>();
        services.AddTransient<CreateIpNetworkDialogView>();
        services.AddTransient<EditIpNetworkDialogView>();
        services.AddTransient<CreateIpNetworkPoolDialogView>();
        services.AddTransient<DeleteIpNetworkPoolDialogView>();
        services.AddTransient<NodeIndexView>();
        services.AddTransient<CreateNodeDialogView>();
        services.AddTransient<EditNodeDialogView>();
        services.AddTransient<NodeMapDialogView>();
        services.AddTransient<NodePingDialogView>();
        services.AddTransient<ConnectionMikrotikControlIndexView>();
        services.AddTransient<CreateConnectionMikrotikControlDialogView>();
        services.AddTransient<EditConnectionMikrotikControlDialogView>();
        services.AddTransient<QueueTypeIndexView>();
        services.AddTransient<CreateQueueTypeDialogView>();
        services.AddTransient<EditQueueTypeDialogView>();
        services.AddTransient<SharedModalWindow>();
        services.AddTransient<SharedAlertWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var fontUri = new Uri(
            "pack://application:,,,/Spix.AppWpf;component/Assets/Fonts/");

        FontsManager.RegisterFont(
            FontAwesomeIconStyle.Solid,
            new FontFamily(
                fontUri,
                "./#Font Awesome 7 Free Solid"));

        FontsManager.RegisterFont(
            FontAwesomeIconStyle.Regular,
            new FontFamily(
                fontUri,
                "./#Font Awesome 7 Free Regular"));

        FontsManager.RegisterFont(
            FontAwesomeIconStyle.Brands,
            new FontFamily(
                fontUri,
                "./#Font Awesome 7 Brands Regular"));

        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        MainWindow = loginWindow;
        loginWindow.Show();
    }

    // Libera los servicios creados durante el ciclo de vida de la aplicacion.
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
