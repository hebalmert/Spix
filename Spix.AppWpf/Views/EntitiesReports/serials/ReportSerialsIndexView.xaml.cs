using Spix.AppWpf.ViewModels.EntitiesReports.serials;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.serials;

// Inventario de seriales. Al abrir se baja el tablero y la tabla: el reporte no tiene
// filtros, asi que no hay nada que esperar del usuario.
public partial class ReportSerialsIndexView : UserControl
{
    private readonly ReportSerialsIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportSerialsIndexView(ReportSerialsIndexViewModel viewModel)
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
