using Microsoft.Extensions.DependencyInjection;
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
using Spix.AppWpf.Views.EntitiesNet.Server;
using Spix.AppWpf.Views.EntitiesSchedule;
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
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
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
    private void ToggleConfigurationClick(object sender, RoutedEventArgs e)
    {
        bool shouldOpen = ConfigurationMenu.Visibility != Visibility.Visible;
        ConfigurationMenu.Visibility = shouldOpen
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (shouldOpen)
        {
            InventoryMenu.Visibility = Visibility.Collapsed;
            NetworkMenu.Visibility = Visibility.Collapsed;
            OperationsMenu.Visibility = Visibility.Collapsed;
        }
    }

    // Abre inventario y contrae los demas grupos para mantener un menu tipo acordeon.
    private void ToggleInventoryClick(object sender, RoutedEventArgs e)
    {
        bool shouldOpen = InventoryMenu.Visibility != Visibility.Visible;
        InventoryMenu.Visibility = shouldOpen
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (shouldOpen)
        {
            ConfigurationMenu.Visibility = Visibility.Collapsed;
            NetworkMenu.Visibility = Visibility.Collapsed;
            OperationsMenu.Visibility = Visibility.Collapsed;
        }
    }

    // Abre Network y contrae los otros grupos para conservar el menu tipo acordeon.
    private void ToggleNetworkClick(object sender, RoutedEventArgs e)
    {
        bool shouldOpen = NetworkMenu.Visibility != Visibility.Visible;
        NetworkMenu.Visibility = shouldOpen
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (shouldOpen)
        {
            ConfigurationMenu.Visibility = Visibility.Collapsed;
            InventoryMenu.Visibility = Visibility.Collapsed;
            OperationsMenu.Visibility = Visibility.Collapsed;
        }
    }

    // Abre Operaciones y mantiene los demas grupos contraidos como en el menu de Blazor.
    private void ToggleOperationsClick(object sender, RoutedEventArgs e)
    {
        bool shouldOpen = OperationsMenu.Visibility != Visibility.Visible;
        OperationsMenu.Visibility = shouldOpen
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (shouldOpen)
        {
            ConfigurationMenu.Visibility = Visibility.Collapsed;
            InventoryMenu.Visibility = Visibility.Collapsed;
            NetworkMenu.Visibility = Visibility.Collapsed;
        }
    }

    // Restaura el tablero sin conservar una vista de entidad dentro del contenedor central.
    private void ShowDashboardClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = DashboardContent;
        MainContent.Margin = new Thickness(0);
        UpdatePageHeader("Home", "Panel principal");
    }

    // Abre los tipos de documento con la misma consulta paginada usada en Blazor.
    private void ShowDocumentTypesClick(object sender, RoutedEventArgs e)
    {
        ShowView<DocumentTypeIndexView>("Tipo documento", "Configuracion / Tipo documento");
    }

    // Abre los estratos sociales con su propio endpoint existente.
    private void ShowEstratosSocialesClick(object sender, RoutedEventArgs e)
    {
        ShowView<EstratoSocialIndexView>("Estrato social", "Configuracion / Estrato social");
    }

    // Muestra las categorias y los planes expandibles en el mismo indice.
    private void ShowPlansClick(object sender, RoutedEventArgs e)
    {
        ShowView<PlanIndexView>("Categoria/Planes", "Configuracion / Categoria/Planes");
    }

    // Muestra las categorias y los servicios expandibles en el mismo indice.
    private void ShowServicesClick(object sender, RoutedEventArgs e)
    {
        ShowView<ServiceIndexView>("Categoria/Servicios", "Configuracion / Categoria/Servicios");
    }

    // Abre las categorias y productos con el mismo acordeon usado por Blazor.
    private void ShowProductsClick(object sender, RoutedEventArgs e)
    {
        ShowView<ProductIndexView>(
            "Categoria/Productos",
            "Inventario / Categoria y productos");
    }

    // Abre las marcas y modelos con el mismo patron expandible de Blazor.
    private void ShowMarksClick(object sender, RoutedEventArgs e)
    {
        ShowView<MarkIndexView>(
            "Marca/Modelo",
            "Inventario / Marcas y modelos");
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

    // Resuelve cada vista con su ViewModel inyectado para mantener la navegacion centralizada.
    private void ShowView<TView>(string title, string subtitle)
        where TView : UserControl
    {
        ShowView(_serviceProvider.GetRequiredService<TView>(), title, subtitle);
    }

    // Mantiene un unico punto para presentar vistas resueltas por inyeccion de dependencias.
    private void ShowView(UserControl view, string title, string subtitle)
    {
        MainContent.Content = view;
        MainContent.Margin = new Thickness(24, 20, 24, 22);
        UpdatePageHeader(title, subtitle);
    }

    // Mantiene el encabezado global sincronizado con la vista que el usuario abre.
    private void UpdatePageHeader(string title, string subtitle)
    {
        PageTitleText.Text = title;
        PageSubtitleText.Text = subtitle;
    }

    // Desconecta eventos para que ninguna ventana cerrada retenga referencias de sesion.
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.ChangePasswordRequested -= OpenChangePassword;
        _viewModel.LogoutRequested -= Logout;
        base.OnClosed(e);
    }
}
