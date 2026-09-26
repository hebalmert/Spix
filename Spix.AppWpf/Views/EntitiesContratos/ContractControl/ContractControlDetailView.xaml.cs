using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

// La configuracion del servicio de un contrato. Se abre desde el listado, con su id.
public partial class ContractControlDetailView : UserControl
{
    private readonly ContractControlDetailViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    // La pantalla no sabe volver: lo decide quien la abrio
    public event EventHandler? BackRequested;

    public ContractControlDetailView(ContractControlDetailViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.BackRequested += (_, _) => BackRequested?.Invoke(this, EventArgs.Empty);

        Loaded += CargarPantalla;
    }

    // Se le dice de que contrato es ANTES de mostrarla
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
