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
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(
        MainWindowViewModel viewModel,
        IServiceProvider serviceProvider,
        NavigationService navigationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;

        //Las pantallas que se abren desde una fila (la orden de trabajo) entran por aqui
        navigationService.Requested += AtenderNavegacion;
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
        ShowView(peticion.View, peticion.Title, peticion.Subtitle);
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
