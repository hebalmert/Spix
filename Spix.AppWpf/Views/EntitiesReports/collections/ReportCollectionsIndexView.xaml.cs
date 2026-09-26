using Spix.AppWpf.ViewModels.EntitiesReports.collections;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.collections;

// Recaudo del periodo. Abre buscando el dia de hoy, que es lo que mas se consulta.
public partial class ReportCollectionsIndexView : UserControl
{
    private readonly ReportCollectionsIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportCollectionsIndexView(ReportCollectionsIndexViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += CargarPantalla;
    }

    //La bandera evita repetir la consulta cuando la pantalla vuelve a mostrarse
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
