using Spix.AppWpf.ViewModels.EntitiesBilling.BillingNote;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesBilling.BillingNote;

// Notas de cobro generales. Al abrir se piden los meses, el tablero del ano y la primera
// pagina, en ese orden.
public partial class BillingNoteIndexView : UserControl
{
    private readonly BillingNoteIndexViewModel _viewModel;
    private bool _isLoaded;

    public BillingNoteIndexView(BillingNoteIndexViewModel viewModel)
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
