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
using Spix.AppWpf.ViewModels.EntitiesGen.Register;
using Spix.AppWpf.ViewModels.EntitiesGen.Tax;
using Spix.AppWpf.ViewModels.EntitiesEmails.EmailProvider;
using Spix.AppWpf.ViewModels.EntitiesGen.Zone;
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
using Spix.AppWpf.ViewModels.EntitiesNet.NodeMap;
using Spix.AppWpf.ViewModels.EntitiesNet.Server;
using Spix.AppWpf.ViewModels.EntitiesSchedule;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using Spix.AppWpf.ViewModels.EntitiesPayment.ContractExonerated;
using Spix.AppWpf.ViewModels.EntitiesPayment.ContractorCxC;
using Spix.AppWpf.ViewModels.EntitiesPayment.CxCBill;
using Spix.AppWpf.ViewModels.EntitiesPayment.PrePayment;
using Spix.AppWpf.ViewModels.EntitiesBilling.BillingNote;
using Spix.AppWpf.ViewModels.EntitiesBilling.BillingNoteOne;
using Spix.AppWpf.ViewModels.EntitiesReports.collections;
using Spix.AppWpf.ViewModels.EntitiesReports.aging;
using Spix.AppWpf.ViewModels.EntitiesReports.commissions;
using Spix.AppWpf.ViewModels.EntitiesReports.audit;
using Spix.AppWpf.ViewModels.EntitiesReports.contracts;
using Spix.AppWpf.ViewModels.EntitiesReports.services;
using Spix.AppWpf.ViewModels.EntitiesReports.cutoff;
using Spix.AppWpf.ViewModels.EntitiesReports.churn;
using Spix.AppWpf.ViewModels.EntitiesReports.activecontracts;
using Spix.AppWpf.ViewModels.EntitiesReports.byzone;
using Spix.AppWpf.ViewModels.EntitiesReports.bynode;
using Spix.AppWpf.ViewModels.EntitiesReports.byserver;
using Spix.AppWpf.ViewModels.EntitiesReports.serials;
using Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;
using Spix.AppWpf.ViewModels.EntitiesSystem.Contractor;
using Spix.AppWpf.ViewModels.EntitiesSystem.Technitian;
using Spix.AppWpf.ViewModels.EntitiesBilling.Sell;
using Spix.AppWpf.ViewModels.EntitiesPayment.TechnicianCollection;
using Spix.AppWpf.ViewModels.EntitiesContratos.Activation;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractExempt;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractSuspended;
using Spix.AppWpf.ViewModels.EntitiesContratos.RunSuspended;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractSuspendedAudit;
using Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;
using Spix.AppWpf.ViewModels.EntitiesOper.Client;
using Spix.AppWpf.ViewModels.EntitiesMK.ConnectionMikrotikControl;
using Spix.AppWpf.ViewModels.EntitiesMK.QueueType;
using Spix.AppWpf.ViewModels.Shell;
using Spix.AppWpf.Views.Auth;
using Spix.AppWpf.Views.EntitiesGen.DocumentType;
using Spix.AppWpf.Views.EntitiesGen.Register;
using Spix.AppWpf.Views.EntitiesGen.Tax;
using Spix.AppWpf.Views.EntitiesEmails.EmailProvider;
using Spix.AppWpf.Views.EntitiesGen.Zone;
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
using Spix.AppWpf.Views.EntitiesNet.NodeMap;
using Spix.AppWpf.Views.EntitiesNet.Server;
using Spix.AppWpf.Views.EntitiesSchedule;
using Spix.AppWpf.Views.EntitiesContratos.ContractClient;
using Spix.AppWpf.Views.EntitiesContratos.ContractControl;
using Spix.AppWpf.Views.EntitiesPayment.ContractExonerated;
using Spix.AppWpf.Views.EntitiesPayment.ContractorCxC;
using Spix.AppWpf.Views.EntitiesPayment.CxCBill;
using Spix.AppWpf.Views.EntitiesPayment.PrePayment;
using Spix.AppWpf.Views.EntitiesBilling.BillingNote;
using Spix.AppWpf.Views.EntitiesBilling.BillingNoteOne;
using Spix.AppWpf.Views.EntitiesReports.collections;
using Spix.AppWpf.Views.EntitiesReports.aging;
using Spix.AppWpf.Views.EntitiesReports.commissions;
using Spix.AppWpf.Views.EntitiesReports.audit;
using Spix.AppWpf.Views.EntitiesReports.contracts;
using Spix.AppWpf.Views.EntitiesReports.services;
using Spix.AppWpf.Views.EntitiesReports.cutoff;
using Spix.AppWpf.Views.EntitiesReports.churn;
using Spix.AppWpf.Views.EntitiesReports.activecontracts;
using Spix.AppWpf.Views.EntitiesReports.byzone;
using Spix.AppWpf.Views.EntitiesReports.bynode;
using Spix.AppWpf.Views.EntitiesReports.byserver;
using Spix.AppWpf.Views.EntitiesReports.serials;
using Spix.AppWpf.Views.EntitiesSystem.Usuario;
using Spix.AppWpf.Views.EntitiesSystem.Contractor;
using Spix.AppWpf.Views.EntitiesSystem.Technitian;
using Spix.AppWpf.Views.EntitiesBilling.Sell;
using Spix.AppWpf.Views.EntitiesPayment.TechnicianCollection;
using Spix.AppWpf.Views.EntitiesContratos.Activation;
using Spix.AppWpf.Views.EntitiesContratos.ContractExempt;
using Spix.AppWpf.Views.EntitiesContratos.ContractDocumentTemplate;
using Spix.AppWpf.Views.EntitiesContratos.ContractSuspended;
using Spix.AppWpf.Views.EntitiesContratos.RunSuspended;
using Spix.AppWpf.Views.EntitiesContratos.ContractSuspendedAudit;
using Spix.AppWpf.Views.EntitiesSchedule.ServiceRequest;
using Spix.AppWpf.Views.EntitiesOper.Client;
using Spix.AppWpf.Views.EntitiesMK.ConnectionMikrotikControl;
using Spix.AppWpf.Views.EntitiesMK.QueueType;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.Services.Network;
using Spix.AppWpf.NetHelper;
using Spix.xNetwork.PingHelper;
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

        //El escritorio abre con los colores de la web, para que el cliente vea el mismo
        //producto entre en el navegador o en el programa. Con el boton de la barra
        //superior se pasa al oscuro.
        AppearanceService.SetLightTheme(true);

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

        //Los mapas se identifican ante OpenStreetMap con la direccion del producto
        WebViewEnvironment.SitioWeb = apiSettings.BaseUrl;

        var services = new ServiceCollection();

        // Registra objetos compartidos para todas las vistas y servicios desktop.
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(apiSettings);
        services.AddSingleton(new HttpClient
        {
            BaseAddress = baseUri
        });
        //El idioma en que responde el Backend; se cambia desde la barra superior
        services.AddSingleton<LanguageService>();
        services.AddSingleton<IUserSessionService, UserSessionService>();
        services.AddSingleton<IRepository>(serviceProvider => new Repository(
            serviceProvider.GetRequiredService<HttpClient>(),
            serviceProvider.GetRequiredService<IUserSessionService>().GetTokenAsync));
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IAccountService, AccountService>();
        services.AddSingleton(typeof(IPagedEntityService<>), typeof(PagedEntityService<>));
        services.AddSingleton<ModalService>();
        services.AddSingleton<NavigationService>();
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
        services.AddTransient<EmailProviderIndexViewModel>();
        services.AddTransient<CreateEmailProviderDialogViewModel>();
        services.AddTransient<EditEmailProviderDialogViewModel>();
        services.AddTransient<ZoneIndexViewModel>();
        services.AddTransient<CreateZoneDialogViewModel>();
        services.AddTransient<EditZoneDialogViewModel>();
        services.AddTransient<RegisterIndexViewModel>();
        services.AddTransient<CreateRegisterDialogViewModel>();
        services.AddTransient<EditRegisterDialogViewModel>();
        services.AddTransient<TaxIndexViewModel>();
        services.AddTransient<CreateTaxDialogViewModel>();
        services.AddTransient<EditTaxDialogViewModel>();
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
        services.AddTransient<ProductStockDialogViewModel>();
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
        services.AddTransient<NodeMapViewModel>();
        services.AddTransient<ServerIndexViewModel>();
        services.AddTransient<CreateServerDialogViewModel>();
        services.AddTransient<EditServerDialogViewModel>();
        services.AddTransient<ServerPingDialogViewModel>();
        services.AddTransient<ServerMikrotikDialogViewModel>();
        services.AddTransient<ScheduleIndexViewModel>();
        services.AddTransient<ContractControlIndexViewModel>();
        services.AddTransient<ContractSuspendedIndexViewModel>();
        services.AddTransient<ContractDocumentTemplateIndexViewModel>();
        services.AddTransient<CreateContractDocumentTemplateDialogViewModel>();
        services.AddTransient<EditContractDocumentTemplateDialogViewModel>();
        services.AddTransient<ContractDocumentTemplateFieldsViewModel>();
        services.AddTransient<ContractDocumentTemplatePdfViewModel>();
        services.AddTransient<RunSuspendedIndexViewModel>();
        services.AddTransient<RunSuspendedFormDialogViewModel>();
        services.AddTransient<RunSuspendedDetailDialogViewModel>();
        services.AddTransient<ActivationIndexViewModel>();
        services.AddTransient<SellIndexViewModel>();
        services.AddTransient<UsuarioIndexViewModel>();
        services.AddTransient<CreateUsuarioDialogViewModel>();
        services.AddTransient<EditUsuarioDialogViewModel>();
        services.AddTransient<UsuarioRoleDetailViewModel>();
        services.AddTransient<CreateUsuarioRoleDialogViewModel>();
        services.AddTransient<ContractorIndexViewModel>();
        services.AddTransient<CreateContractorDialogViewModel>();
        services.AddTransient<EditContractorDialogViewModel>();
        services.AddTransient<TechnitianIndexViewModel>();
        services.AddTransient<CreateTechnitianDialogViewModel>();
        services.AddTransient<EditTechnitianDialogViewModel>();
        services.AddTransient<ReportCollectionsIndexViewModel>();
        services.AddTransient<ReportAgingIndexViewModel>();
        services.AddTransient<ReportCommissionsIndexViewModel>();
        services.AddTransient<ReportAuditIndexViewModel>();
        services.AddTransient<ReportContractsIndexViewModel>();
        services.AddTransient<ReportServicesIndexViewModel>();
        services.AddTransient<ReportCutOffIndexViewModel>();
        services.AddTransient<ReportChurnIndexViewModel>();
        services.AddTransient<ReportActiveContractsIndexViewModel>();
        services.AddTransient<ReportByZoneIndexViewModel>();
        services.AddTransient<ReportByNodeIndexViewModel>();
        services.AddTransient<ReportByServerIndexViewModel>();
        services.AddTransient<ReportSerialsIndexViewModel>();
        services.AddTransient<TechnicianCollectionIndexViewModel>();
        services.AddTransient<ContractExemptIndexViewModel>();
        services.AddTransient<CxCBillIndexViewModel>();
        services.AddTransient<ContractorCxCIndexViewModel>();
        services.AddTransient<CreateContractorCxCDialogViewModel>();
        services.AddTransient<ContractorCxCCommissionsDialogViewModel>();
        services.AddTransient<ContractorCxCPaymentsDialogViewModel>();
        services.AddTransient<ContractorCxCPayDialogViewModel>();
        services.AddTransient<ContractorCxCCancelDialogViewModel>();
        services.AddTransient<CxCBillDetailDialogViewModel>();
        services.AddTransient<CxCBillPayDialogViewModel>();
        services.AddTransient<CxCBillCancelDialogViewModel>();
        services.AddTransient<ContractExoneratedIndexViewModel>();
        services.AddTransient<CreateContractExoneratedDialogViewModel>();
        services.AddTransient<EditContractExoneratedDialogViewModel>();
        services.AddTransient<PrePaymentIndexViewModel>();
        services.AddTransient<PrePaymentDialogViewModel>();
        services.AddTransient<BillingNoteIndexViewModel>();
        services.AddTransient<CreateBillingNoteDialogViewModel>();
        services.AddTransient<EditBillingNoteDialogViewModel>();
        services.AddTransient<BillingNoteDetailDialogViewModel>();
        services.AddTransient<BillingNoteOneIndexViewModel>();
        services.AddTransient<BillingNoteOneFormDialogViewModel>();
        services.AddTransient<BillingNoteOneDetailDialogViewModel>();
        services.AddTransient<ContractExemptDialogViewModel>();
        services.AddTransient<SellDetailDialogViewModel>();
        services.AddTransient<ContractSuspendedAuditIndexViewModel>();
        services.AddTransient<ContractSuspendedDialogViewModel>();
        services.AddTransient<ContractControlDetailViewModel>();
        services.AddTransient<ContractServerDialogViewModel>();
        services.AddTransient<ContractIpDialogViewModel>();
        services.AddTransient<ContractNodeDialogViewModel>();
        services.AddTransient<ContractMacDialogViewModel>();
        services.AddTransient<ContractPlanDialogViewModel>();
        services.AddTransient<ContractQueueDialogViewModel>();
        services.AddTransient<ContractBindDialogViewModel>();
        services.AddTransient<ContractMapDialogViewModel>();
        services.AddTransient<ContractChangeStateDialogViewModel>();
        services.AddTransient<ContractClientIndexViewModel>();
        services.AddTransient<CreateContractClientDialogViewModel>();
        services.AddTransient<EditContractClientDialogViewModel>();
        services.AddTransient<ContractAuditDialogViewModel>();
        services.AddTransient<ContractIdPicsDialogViewModel>();
        services.AddTransient<ContractDocumentDialogViewModel>();
        services.AddTransient<ServiceRequestIndexViewModel>();
        services.AddTransient<CreateServiceRequestDialogViewModel>();
        services.AddTransient<ServiceRequestOrderViewModel>();
        services.AddTransient<AssignServiceRequestDialogViewModel>();
        services.AddTransient<ResolveByPhoneDialogViewModel>();
        services.AddTransient<UploadServicePhotoDialogViewModel>();
        services.AddTransient<ServicePhotoViewerDialogViewModel>();
        services.AddTransient<CreateScheduleDialogViewModel>();
        services.AddTransient<EditScheduleDialogViewModel>();
        services.AddTransient<ServiceRequestScheduleInfoDialogViewModel>();
        services.AddTransient<ClientIndexViewModel>();
        services.AddTransient<CreateClientDialogViewModel>();
        services.AddTransient<EditClientDialogViewModel>();
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
        services.AddTransient<EmailProviderIndexView>();
        services.AddTransient<CreateEmailProviderDialogView>();
        services.AddTransient<EditEmailProviderDialogView>();
        services.AddTransient<ZoneIndexView>();
        services.AddTransient<CreateZoneDialogView>();
        services.AddTransient<EditZoneDialogView>();
        services.AddTransient<RegisterIndexView>();
        services.AddTransient<CreateRegisterDialogView>();
        services.AddTransient<EditRegisterDialogView>();
        services.AddTransient<TaxIndexView>();
        services.AddTransient<CreateTaxDialogView>();
        services.AddTransient<EditTaxDialogView>();
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
        services.AddTransient<ProductStockDialogView>();
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
        services.AddTransient<NodeMapView>();
        services.AddTransient<ServerIndexView>();
        services.AddTransient<CreateServerDialogView>();
        services.AddTransient<EditServerDialogView>();
        services.AddTransient<ServerPingDialogView>();
        services.AddTransient<ServerMikrotikDialogView>();
        services.AddTransient<ScheduleIndexView>();
        services.AddTransient<ContractControlIndexView>();
        services.AddTransient<ContractSuspendedIndexView>();
        services.AddTransient<ContractDocumentTemplateIndexView>();
        services.AddTransient<CreateContractDocumentTemplateDialogView>();
        services.AddTransient<EditContractDocumentTemplateDialogView>();
        services.AddTransient<ContractDocumentTemplateFieldsDialogView>();
        services.AddTransient<ContractDocumentTemplatePdfDialogView>();
        services.AddTransient<RunSuspendedIndexView>();
        services.AddTransient<RunSuspendedFormDialogView>();
        services.AddTransient<RunSuspendedDetailDialogView>();
        services.AddTransient<ActivationIndexView>();
        services.AddTransient<SellIndexView>();
        services.AddTransient<UsuarioIndexView>();
        services.AddTransient<CreateUsuarioDialogView>();
        services.AddTransient<EditUsuarioDialogView>();
        services.AddTransient<UsuarioRoleDetailView>();
        services.AddTransient<CreateUsuarioRoleDialogView>();
        services.AddTransient<ContractorIndexView>();
        services.AddTransient<CreateContractorDialogView>();
        services.AddTransient<EditContractorDialogView>();
        services.AddTransient<TechnitianIndexView>();
        services.AddTransient<CreateTechnitianDialogView>();
        services.AddTransient<EditTechnitianDialogView>();
        services.AddTransient<ReportCollectionsIndexView>();
        services.AddTransient<ReportAgingIndexView>();
        services.AddTransient<ReportCommissionsIndexView>();
        services.AddTransient<ReportAuditIndexView>();
        services.AddTransient<ReportContractsIndexView>();
        services.AddTransient<ReportServicesIndexView>();
        services.AddTransient<ReportCutOffIndexView>();
        services.AddTransient<ReportChurnIndexView>();
        services.AddTransient<ReportActiveContractsIndexView>();
        services.AddTransient<ReportByZoneIndexView>();
        services.AddTransient<ReportByNodeIndexView>();
        services.AddTransient<ReportByServerIndexView>();
        services.AddTransient<ReportSerialsIndexView>();
        services.AddTransient<TechnicianCollectionIndexView>();
        services.AddTransient<ContractExemptIndexView>();
        services.AddTransient<CxCBillIndexView>();
        services.AddTransient<ContractorCxCIndexView>();
        services.AddTransient<CreateContractorCxCDialogView>();
        services.AddTransient<ContractorCxCCommissionsDialogView>();
        services.AddTransient<ContractorCxCPaymentsDialogView>();
        services.AddTransient<ContractorCxCPayDialogView>();
        services.AddTransient<ContractorCxCCancelDialogView>();
        services.AddTransient<CxCBillDetailDialogView>();
        services.AddTransient<CxCBillPayDialogView>();
        services.AddTransient<CxCBillCancelDialogView>();
        services.AddTransient<ContractExoneratedIndexView>();
        services.AddTransient<CreateContractExoneratedDialogView>();
        services.AddTransient<EditContractExoneratedDialogView>();
        services.AddTransient<PrePaymentIndexView>();
        services.AddTransient<PrePaymentDialogView>();
        services.AddTransient<BillingNoteIndexView>();
        services.AddTransient<CreateBillingNoteDialogView>();
        services.AddTransient<EditBillingNoteDialogView>();
        services.AddTransient<BillingNoteDetailDialogView>();
        services.AddTransient<BillingNoteOneIndexView>();
        services.AddTransient<BillingNoteOneFormDialogView>();
        services.AddTransient<BillingNoteOneDetailDialogView>();
        services.AddTransient<ContractExemptDialogView>();
        services.AddTransient<SellDetailDialogView>();
        services.AddTransient<ContractSuspendedAuditIndexView>();
        services.AddTransient<ContractSuspendedDialogView>();
        services.AddTransient<ContractControlDetailView>();
        services.AddTransient<ContractServerDialogView>();
        services.AddTransient<ContractIpDialogView>();
        services.AddTransient<ContractNodeDialogView>();
        services.AddTransient<ContractMacDialogView>();
        services.AddTransient<ContractPlanDialogView>();
        services.AddTransient<ContractQueueDialogView>();
        services.AddTransient<ContractBindDialogView>();
        services.AddTransient<ContractMapDialogView>();
        services.AddTransient<ContractMapViewDialogView>();
        services.AddTransient<ContractChangeStateDialogView>();
        services.AddTransient<ContractClientIndexView>();
        services.AddTransient<CreateContractClientDialogView>();
        services.AddTransient<EditContractClientDialogView>();
        services.AddTransient<ContractAuditDialogView>();
        services.AddTransient<ContractIdPicsDialogView>();
        services.AddTransient<ContractDocumentDialogView>();
        services.AddTransient<ServiceRequestIndexView>();
        services.AddTransient<CreateServiceRequestDialogView>();
        services.AddTransient<ServiceRequestOrderView>();
        services.AddTransient<AssignServiceRequestDialogView>();
        services.AddTransient<ResolveByPhoneDialogView>();
        services.AddTransient<UploadServicePhotoDialogView>();
        services.AddTransient<ServicePhotoViewerDialogView>();
        services.AddTransient<CreateScheduleDialogView>();
        services.AddTransient<EditScheduleDialogView>();
        services.AddTransient<ServiceRequestScheduleInfoDialogView>();
        services.AddTransient<ClientIndexView>();
        services.AddTransient<CreateClientDialogView>();
        services.AddTransient<EditClientDialogView>();
        services.AddTransient<ConnectionMikrotikControlIndexView>();
        services.AddTransient<CreateConnectionMikrotikControlDialogView>();
        services.AddTransient<EditConnectionMikrotikControlDialogView>();
        services.AddTransient<QueueTypeIndexView>();
        services.AddTransient<CreateQueueTypeDialogView>();
        services.AddTransient<EditQueueTypeDialogView>();
        services.AddTransient<SharedModalWindow>();
        services.AddTransient<SharedAlertWindow>();

        _serviceProvider = services.BuildServiceProvider();

        //Se despierta de una para que la cabecera del idioma quede puesta antes del
        //primer llamado: si no, el login todavia responderia en el idioma por defecto.
        _serviceProvider.GetRequiredService<LanguageService>();

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
