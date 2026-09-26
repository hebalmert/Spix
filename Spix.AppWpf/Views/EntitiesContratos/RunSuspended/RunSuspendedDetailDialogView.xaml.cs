using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.RunSuspended;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.RunSuspended;

// El detalle del corte: la revision previa y la ejecucion.
public partial class RunSuspendedDetailDialogView : UserControl, ISharedModalContent
{
    private readonly RunSuspendedDetailDialogViewModel _viewModel;
    private bool _isLoaded;

    public RunSuspendedDetailDialogView(RunSuspendedDetailDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarPantalla;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null &&
            parameters.TryGetValue("Id", out var valor) &&
            valor is Guid id)
        {
            _viewModel.SetId(id);
        }
    }

    private async void CargarPantalla(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync();
    }
}
