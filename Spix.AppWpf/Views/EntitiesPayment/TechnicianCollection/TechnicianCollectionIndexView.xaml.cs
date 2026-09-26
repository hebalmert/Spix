using Spix.AppWpf.ViewModels.EntitiesPayment.TechnicianCollection;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.TechnicianCollection;

// Cobro tecnico. Al abrir solo se baja la lista de quienes cobran: lo demas espera a Buscar.
public partial class TechnicianCollectionIndexView : UserControl
{
    private readonly TechnicianCollectionIndexViewModel _viewModel;
    private bool _isLoaded;

    public TechnicianCollectionIndexView(TechnicianCollectionIndexViewModel viewModel)
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
