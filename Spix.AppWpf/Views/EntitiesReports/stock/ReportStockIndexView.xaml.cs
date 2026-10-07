using Spix.AppWpf.ViewModels.EntitiesReports.stock;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.stock;

// Movimientos de inventario. Al abrir se baja el combo de bodegas, el tablero y las dos
// tablas; despues el usuario filtra por periodo o bodega y vuelve a pedirlo.
public partial class ReportStockIndexView : UserControl
{
    private readonly ReportStockIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportStockIndexView(ReportStockIndexViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += CargarPantalla;
    }

    //Loaded se dispara cada vez que la vista se vuelve a mostrar: la bandera evita pedir
    //el reporte otra vez por solo volver a la pantalla
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
