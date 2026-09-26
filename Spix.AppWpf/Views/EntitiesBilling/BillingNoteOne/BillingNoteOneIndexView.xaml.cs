using Spix.AppWpf.ViewModels.EntitiesBilling.BillingNoteOne;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesBilling.BillingNoteOne;

// Nota de cobro individual. Al abrir se piden el combo de meses, el tablero y la lista.
public partial class BillingNoteOneIndexView : UserControl
{
    private readonly BillingNoteOneIndexViewModel _viewModel;
    private bool _isLoaded;

    public BillingNoteOneIndexView(BillingNoteOneIndexViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += CargarPantalla;
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
