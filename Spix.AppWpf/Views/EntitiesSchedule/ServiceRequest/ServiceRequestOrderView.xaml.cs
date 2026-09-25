using Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule.ServiceRequest;

// La orden de trabajo de una solicitud. Se abre desde el listado, con su id.
public partial class ServiceRequestOrderView : UserControl
{
    private readonly ServiceRequestOrderViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    // La pantalla no sabe volver: lo decide quien la abrio
    public event EventHandler? BackRequested;

    public ServiceRequestOrderView(ServiceRequestOrderViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.BackRequested += (_, _) => BackRequested?.Invoke(this, EventArgs.Empty);

        Loaded += CargarPantalla;
    }

    // Se le dice de que orden es ANTES de mostrarla
    public void Prepare(Guid id)
    {
        _id = id;
    }

    private async void CargarPantalla(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync(_id);
    }
}
