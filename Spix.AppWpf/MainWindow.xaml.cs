using FontAwesome.Net.Generators;
using Microsoft.Extensions.DependencyInjection;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shell;
using Spix.AppWpf.Views.Auth;
using Spix.AppWpf.Views.EntitiesGen.DocumentType;
using Spix.AppWpf.Views.EntitiesGen.EstratoSocial;
using Spix.AppWpf.Views.EntitiesGen.Register;
using Spix.AppWpf.Views.EntitiesGen.Tax;
using Spix.AppWpf.Views.EntitiesEmails.EmailProvider;
using Spix.AppWpf.Views.EntitiesGen.Zone;
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
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf;

// Contiene el entorno principal y coordina las acciones globales de la sesion.
public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LanguageService _languageService;

    //Como volver a levantar la pantalla que se esta viendo, para cuando cambia el idioma
    private Func<UserControl>? _rehacerVista;
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(
        MainWindowViewModel viewModel,
        IServiceProvider serviceProvider,
        NavigationService navigationService,
        LanguageService languageService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;

        //Las pantallas que se abren desde una fila (la orden de trabajo) entran por aqui
        navigationService.Requested += AtenderNavegacion;

        //El idioma arranca en ingles, que es lo que responde el Backend por defecto
        _languageService = languageService;
        LanguageText.Text = _languageService.Current.ToUpperInvariant();
        //El boton muestra en que tema esta parado
        ThemeIcon.Icon = AppearanceService.IsLight ? FontAwesomeIcon.Sun : FontAwesomeIcon.Moon;

        _viewModel.ChangePasswordRequested += OpenChangePassword;
        _viewModel.LogoutRequested += Logout;
        DataContext = _viewModel;
    }

    // Muestra el formulario seguro de clave sin perder la ventana principal.
    private void OpenChangePassword(object? sender, EventArgs e)
    {
        var changePasswordWindow = _serviceProvider.GetRequiredService<ChangePasswordWindow>();
        changePasswordWindow.Owner = this;
        changePasswordWindow.ShowDialog();
    }

    // Regresa al acceso luego de liberar el token guardado en memoria.
    private void Logout(object? sender, EventArgs e)
    {
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }

    // Abre configuracion y contrae los demas grupos para mantener un menu tipo acordeon.
    // Abre un grupo del menu y cierra los demas: acordeon.
    //
    // Un solo metodo para los ocho grupos. El boton dice en su Tag el nombre del panel
    // que le toca, asi agregar un grupo nuevo no obliga a tocar codigo.
    private void ToggleMenuClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button boton || boton.Tag is not string nombrePanel)
        {
            return;
        }

        var panel = FindName(nombrePanel) as StackPanel;
        if (panel == null)
        {
            return;
        }

        bool abrir = panel.Visibility != Visibility.Visible;

        foreach (var grupo in MenuPanels())
        {
            grupo.Visibility = Visibility.Collapsed;
        }

        panel.Visibility = abrir ? Visibility.Visible : Visibility.Collapsed;
    }

    // Los paneles de los grupos del menu
    private IEnumerable<StackPanel> MenuPanels()
    {
        string[] nombres =
        [
            "ConfigurationMenu", "CatalogMenu", "InventoryMenu", "NetworkMenu",
            "OperationsMenu", "FinanceMenu", "ReportsMenu", "SystemMenu"
        ];

        foreach (var nombre in nombres)
        {
            if (FindName(nombre) is StackPanel panel)
            {
                yield return panel;
            }
        }
    }

    // Vuelve al tablero
    private void ShowDashboardClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = DashboardContent;
        MainContent.Margin = new Thickness(0);
        UpdatePageHeader("Home", "Panel principal", FontAwesomeIcon.House);
    }

    // Abre los tipos de documento con la misma consulta paginada usada en Blazor.
    private void ShowDocumentTypesClick(object sender, RoutedEventArgs e)
    {
        ShowView<DocumentTypeIndexView>("Tipo documento", "Configuracion / Tipo documento", FontAwesomeIcon.IdCard);
    }

    // Abre la configuracion de correo, con el mismo endpoint que usa Blazor.
    private void ShowEmailProvidersClick(object sender, RoutedEventArgs e)
    {
        ShowView<EmailProviderIndexView>("Correo", "Configuracion / Correo", FontAwesomeIcon.Envelope);
    }

    // Abre las zonas, con el mismo endpoint que usa Blazor.
    private void ShowZonesClick(object sender, RoutedEventArgs e)
    {
        ShowView<ZoneIndexView>("Zonas", "Configuracion / Zonas", FontAwesomeIcon.MapLocationDot);
    }

    // Abre los consecutivos, con el mismo endpoint que usa Blazor.
    private void ShowRegistersClick(object sender, RoutedEventArgs e)
    {
        ShowView<RegisterIndexView>("Consecutivos", "Configuracion / Consecutivos", FontAwesomeIcon.ListOl);
    }

    // Abre los impuestos, con el mismo endpoint que usa Blazor.
    private void ShowTaxesClick(object sender, RoutedEventArgs e)
    {
        ShowView<TaxIndexView>("Impuestos", "Configuracion / Impuestos", FontAwesomeIcon.Percent);
    }

    // Abre los estratos sociales con su propio endpoint existente.
    private void ShowEstratosSocialesClick(object sender, RoutedEventArgs e)
    {
        ShowView<EstratoSocialIndexView>("Estrato social", "Configuracion / Estrato social", FontAwesomeIcon.LayerGroup);
    }

    // Categorias a la izquierda y sus planes a la derecha, igual que en la web.
    private void ShowPlansClick(object sender, RoutedEventArgs e)
    {
        ShowView<PlanIndexView>("Categoria/Planes", "Configuracion / Categoria/Planes", FontAwesomeIcon.Wifi);
    }

    // Categorias a la izquierda y sus servicios a la derecha, igual que en la web.
    private void ShowServicesClick(object sender, RoutedEventArgs e)
    {
        ShowView<ServiceIndexView>("Categoria/Servicios", "Configuracion / Categoria/Servicios", FontAwesomeIcon.BellConcierge);
    }

    // Categorias a la izquierda y sus productos a la derecha, igual que en la web.
    private void ShowProductsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ProductIndexView>(
            "Categoria/Productos",
            "Inventario / Categoria y productos",
            FontAwesomeIcon.BoxOpen);
    }

    // Marcas a la izquierda y sus modelos a la derecha, igual que en la web.
    private void ShowMarksClick(object sender, RoutedEventArgs e)
    {
        ShowView<MarkIndexView>(
            "Marca/Modelo",
            "Inventario / Marcas y modelos",
            FontAwesomeIcon.Tags);
    }

    // El mapa del nodo elegido con sus clientes, igual que la pantalla /nodemap de la web.
    private void ShowNodeMapClick(object sender, RoutedEventArgs e)
    {
        ShowView<NodeMapView>(
            "Mapa de nodos",
            "Red / Mapa de nodos",
            FontAwesomeIcon.MapLocationDot);
    }

    // Abre proveedores usando el mismo indice paginado de la aplicacion web.
    private void ShowSuppliersClick(object sender, RoutedEventArgs e)
    {
        ShowView<SupplierIndexView>(
            "Proveedores",
            "Inventario / Proveedores");
    }

    // Abre bodegas y conserva sus datos de ubicacion desde el Backend.
    private void ShowStoragesClick(object sender, RoutedEventArgs e)
    {
        ShowView<ProductStorageIndexView>(
            "Bodegas",
            "Inventario / Bodegas");
    }

    // Abre el indice paginado de compras y conserva la navegacion hacia su detalle.
    private void ShowPurchasesClick(object sender, RoutedEventArgs e)
    {
        var view = _serviceProvider.GetRequiredService<PurchaseIndexView>();
        view.DetailsRequested += ShowPurchaseDetails;
        ShowView(view, "Compras", "Inventario / Compras");
    }

    // Abre el detalle de una compra para administrar sus productos y cerrarla.
    private void ShowPurchaseDetails(object? sender, Guid purchaseId)
    {
        var view = _serviceProvider.GetRequiredService<PurchaseDetailsView>();
        view.BackRequested += ShowPurchasesFromDetails;
        view.LoadPurchase(purchaseId);
        ShowView(view, "Detalle compra", "Inventario / Compras / Detalle");
    }

    // Restablece el indice de compras cuando el usuario termina de revisar el detalle.
    private void ShowPurchasesFromDetails(object? sender, EventArgs e)
    {
        ShowPurchasesClick(this, new RoutedEventArgs());
    }

    // Muestra los cargues pendientes y completados antes de administrar sus MAC.
    private void ShowCarguesClick(object sender, RoutedEventArgs e)
    {
        var view = _serviceProvider.GetRequiredService<CargueIndexView>();
        view.DetailsRequested += ShowCargueDetails;
        ShowView(view, "Cargue seriales", "Inventario / Cargue seriales");
    }

    // Abre la carga de MAC de la recepcion seleccionada.
    private void ShowCargueDetails(object? sender, Guid cargueId)
    {
        var view = _serviceProvider.GetRequiredService<CargueDetailsView>();
        view.BackRequested += ShowCarguesFromDetails;
        view.LoadCargue(cargueId);
        ShowView(view, "Detalle cargue", "Inventario / Cargue seriales / Detalle");
    }

    // Restablece el indice al terminar de administrar un cargue.
    private void ShowCarguesFromDetails(object? sender, EventArgs e)
    {
        ShowCarguesClick(this, new RoutedEventArgs());
    }

    // Abre las direcciones IP que se asignan a clientes desde su endpoint existente.
    private void ShowIpNetsClick(object sender, RoutedEventArgs e)
    {
        ShowView<IpNetIndexView>("IP Clientes", "Network / IP Clientes");
    }

    // Abre las direcciones IP reservadas para nodos y servidores.
    private void ShowIpNetworksClick(object sender, RoutedEventArgs e)
    {
        ShowView<IpNetworkIndexView>("IP Red", "Network / IP Red");
    }

    // Abre el tipo de control MikroTik configurado para la corporacion.
    private void ShowConnectionMikrotikControlsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ConnectionMikrotikControlIndexView>("Control MikroTik", "Network / Control MikroTik");
    }

    // Abre el listado de tipos de Queue usados por la configuracion MikroTik.
    private void ShowQueueTypesClick(object sender, RoutedEventArgs e)
    {
        ShowView<QueueTypeIndexView>("Queue Types", "Network / Queue Types");
    }

    // Abre los nodos de acceso y conserva el formulario completo de configuracion de radio.
    private void ShowNodesClick(object sender, RoutedEventArgs e)
    {
        ShowView<NodeIndexView>("Nodos", "Network / Nodos");
    }

    // Abre los servidores y permite ejecutar los diagnosticos locales desde WPF.
    private void ShowServersClick(object sender, RoutedEventArgs e)
    {
        ShowView<ServerIndexView>("Servidores", "Network / Servidores");
    }

    // Abre el calendario de agendas con los mismos eventos que consulta Blazor.
    private void ShowScheduleClick(object sender, RoutedEventArgs e)
    {
        ShowView<ScheduleIndexView>("Schedule", "Operaciones / Schedule");
    }

    // Abre el indice paginado de clientes junto al resto de operaciones.
    private void ShowClientsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ClientIndexView>("Clientes", "Operaciones / Clientes");
    }

    // Las pantallas que se abren desde una fila (la orden de trabajo) piden por aqui:
    // la ventana principal es la unica que sabe pintar.
    private void AtenderNavegacion(object? sender, NavigationRequest peticion)
    {
        _rehacerVista = peticion.Build;

        ShowView(peticion.Build(), peticion.Title, peticion.Subtitle);
    }

    // Abre el seguimiento de los contratos operativos.
    private void ShowContractControlClick(object sender, RoutedEventArgs e)
    {
        ShowView<ContractControlIndexView>("Control de contratos", "Operaciones / Control de contratos");
    }

    // Abre el registro de las suspensiones, con lo que se suspendio y lo que ya se reactivo.
    private void ShowContractSuspendedClick(object sender, RoutedEventArgs e)
    {
        ShowView<ContractSuspendedIndexView>("Contratos suspendidos", "Operaciones / Contratos suspendidos");
    }

    // Le devuelve el servicio a los que se cortaron y ya pagaron, equipo por equipo.
    private void ShowActivationClick(object sender, RoutedEventArgs e)
    {
        ShowView<ActivationIndexView>("Reactivacion", "Operaciones / Reactivacion");
    }

    // Quien le devolvio el servicio a quien y cuando: solo consulta.
    private void ShowContractSuspendedAuditClick(object sender, RoutedEventArgs e)
    {
        ShowView<ContractSuspendedAuditIndexView>("Auditoria de activaciones", "Operaciones / Auditoria de activaciones");
    }

    //===================== Finanzas =====================

    // Lo que se le facturo a cada cliente: solo consulta.
    private void ShowSellsClick(object sender, RoutedEventArgs e)
    {
        ShowView<SellIndexView>("Facturas", "Finanzas / Facturas");
    }

    // El cuadre con quien recoge la plata en la calle: solo consulta.
    private void ShowTechnicianCollectionsClick(object sender, RoutedEventArgs e)
    {
        ShowView<TechnicianCollectionIndexView>("Cobro tecnico", "Finanzas / Cobro tecnico");
    }

    // Contratos que mantienen el servicio sin que se les cobre.
    private void ShowContractExemptClick(object sender, RoutedEventArgs e)
    {
        ShowView<ContractExemptIndexView>("Exoneracion fija", "Finanzas / Exoneracion fija");
    }

    // La exoneracion de UN mes: tiene vencimiento, a diferencia de la fija.
    private void ShowContractExoneratedClick(object sender, RoutedEventArgs e)
    {
        ShowView<ContractExoneratedIndexView>("Exoneracion del mes", "Finanzas / Exoneracion del mes");
    }

    // Lo que el cliente paga por adelantado y se descuenta al facturar.
    private void ShowPrePaymentsClick(object sender, RoutedEventArgs e)
    {
        ShowView<PrePaymentIndexView>("Pagos adelantados", "Finanzas / Pagos adelantados");
    }

    // La facturacion masiva del mes: de aqui nacen las cuentas por cobrar.
    private void ShowBillingNotesClick(object sender, RoutedEventArgs e)
    {
        ShowView<BillingNoteIndexView>("Notas de cobro", "Finanzas / Notas de cobro");
    }

    // Lo mismo, pero para un solo contrato.
    private void ShowBillingNoteOnesClick(object sender, RoutedEventArgs e)
    {
        ShowView<BillingNoteOneIndexView>("Nota individual", "Finanzas / Nota individual");
    }

    // La caja: lo que cada cliente debe y lo que se le ha recibido.
    private void ShowCxCBillsClick(object sender, RoutedEventArgs e)
    {
        ShowView<CxCBillIndexView>("Cuentas por cobrar", "Finanzas / Cuentas por cobrar");
    }

    // Lo que se le debe a cada contratista por los pagos que recaudo.
    private void ShowContractorCxCClick(object sender, RoutedEventArgs e)
    {
        ShowView<ContractorCxCIndexView>("Pagos a contratistas", "Finanzas / Pagos a contratistas");
    }

    // El corte masivo por falta de pago: le quita el acceso a los que deben.
    private void ShowRunSuspendedClick(object sender, RoutedEventArgs e)
    {
        ShowView<RunSuspendedIndexView>("Corte general", "Finanzas / Corte general");
    }

    //===================== Reportes =====================
    //Todos son de sola lectura: consultan y pintan, no escriben nada.

    private void ShowReportCollectionsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportCollectionsIndexView>("Recaudo del periodo", "Reportes / Recaudo del periodo");
    }

    private void ShowReportAgingClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportAgingIndexView>("Cartera por antiguedad", "Reportes / Cartera por antiguedad");
    }

    private void ShowReportCommissionsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportCommissionsIndexView>("Comisiones de contratistas", "Reportes / Comisiones de contratistas");
    }

    private void ShowReportAuditClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportAuditIndexView>("Bitacora del dinero", "Reportes / Bitacora del dinero");
    }

    private void ShowReportContractsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportContractsIndexView>("Contratos del periodo", "Reportes / Contratos del periodo");
    }

    private void ShowReportServicesClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportServicesIndexView>("Servicios del periodo", "Reportes / Servicios del periodo");
    }

    private void ShowReportCutOffClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportCutOffIndexView>("Efectividad del corte", "Reportes / Efectividad del corte");
    }

    private void ShowReportChurnClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportChurnIndexView>("Fuera de servicio", "Reportes / Fuera de servicio");
    }

    private void ShowReportActiveContractsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportActiveContractsIndexView>("Contratos activos", "Reportes / Contratos activos");
    }

    private void ShowReportByZoneClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportByZoneIndexView>("Contratos por zona", "Reportes / Contratos por zona");
    }

    private void ShowReportByNodeClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportByNodeIndexView>("Contratos por AP", "Reportes / Contratos por AP");
    }

    private void ShowReportByServerClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportByServerIndexView>("Contratos por servidor", "Reportes / Contratos por servidor");
    }

    private void ShowReportSerialsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ReportSerialsIndexView>("Inventario de seriales", "Reportes / Inventario de seriales");
    }

    //===================== Sistema =====================

    private void ShowUsuariosClick(object sender, RoutedEventArgs e)
    {
        ShowView<UsuarioIndexView>("Usuarios", "Sistema / Usuarios");
    }

    private void ShowContractorsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ContractorIndexView>("Contratistas", "Sistema / Contratistas");
    }

    private void ShowTechnitiansClick(object sender, RoutedEventArgs e)
    {
        ShowView<TechnitianIndexView>("Tecnicos", "Sistema / Tecnicos");
    }

    // Las plantillas PDF del contrato y del consentimiento, con sus campos colocados.
    private void ShowContractDocumentTemplatesClick(object sender, RoutedEventArgs e)
    {
        ShowView<ContractDocumentTemplateIndexView>("Plantillas PDF", "Configuracion / Plantillas PDF");
    }

    // Abre los contratos de los clientes.
    private void ShowContractsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ContractClientIndexView>("Contratos", "Operaciones / Contratos");
    }

    // Abre las visitas tecnicas del cliente.
    private void ShowServiceRequestsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ServiceRequestIndexView>("Solicitudes de servicio", "Operaciones / Solicitudes de servicio");
    }

    // Resuelve cada vista con su ViewModel inyectado para mantener la navegacion centralizada.
    private void ShowView<TView>(string title, string subtitle, FontAwesomeIcon? icon = null)
        where TView : UserControl
    {
        _rehacerVista = () => _serviceProvider.GetRequiredService<TView>();

        ShowView(_serviceProvider.GetRequiredService<TView>(), title, subtitle, icon);
    }

    // Mantiene un unico punto para presentar vistas resueltas por inyeccion de dependencias.
    private void ShowView(UserControl view, string title, string subtitle, FontAwesomeIcon? icon = null)
    {
        MainContent.Content = view;
        MainContent.Margin = new Thickness(24, 20, 24, 22);
        UpdatePageHeader(title, subtitle, icon);
    }

    // Mantiene el encabezado global sincronizado con la vista que el usuario abre.
    private void UpdatePageHeader(string title, string subtitle, FontAwesomeIcon? icon = null)
    {
        PageTitleText.Text = title;
        PageSubtitleText.Text = subtitle;
        PageGlyph.Icon = icon ?? FontAwesomeIcon.TableList;
    }

    // El idioma en que responde el Backend. Los combos, los estados y los mensajes de
    // error los manda el servidor ya traducidos, asi que al cambiarlo hay que volver a
    // levantar la pantalla para que los pida de nuevo.
    private void ToggleLanguageClick(object sender, RoutedEventArgs e)
    {
        _languageService.Toggle();

        LanguageText.Text = _languageService.Current.ToUpperInvariant();

        if (_rehacerVista is not null)
        {
            MainContent.Content = _rehacerVista();
        }
    }

    // Tema claro u oscuro para toda la aplicacion, en caliente
    private void ToggleThemeClick(object sender, RoutedEventArgs e)
    {
        AppearanceService.SetLightTheme(!AppearanceService.IsLight);

        ThemeIcon.Icon = AppearanceService.IsLight
            ? FontAwesomeIcon.Sun
            : FontAwesomeIcon.Moon;
    }

    // Filas comodas o compactas en todos los listados
    private void ToggleDensityClick(object sender, RoutedEventArgs e)
    {
        AppearanceService.SetCompact(!AppearanceService.IsCompact);

        DensityIcon.Icon = AppearanceService.IsCompact
            ? FontAwesomeIcon.Bars
            : FontAwesomeIcon.BarsStaggered;
    }

    // Desconecta eventos para que ninguna ventana cerrada retenga referencias de sesion.
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.ChangePasswordRequested -= OpenChangePassword;
        _viewModel.LogoutRequested -= Logout;
        base.OnClosed(e);
    }
}
