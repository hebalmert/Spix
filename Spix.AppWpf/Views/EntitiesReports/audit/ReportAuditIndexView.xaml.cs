using Spix.AppWpf.ViewModels.EntitiesReports.audit;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.audit;

// Bitacora del dinero. Al abrir se bajan los tipos de movimiento y la primera pagina.
public partial class ReportAuditIndexView : UserControl
{
    private readonly ReportAuditIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportAuditIndexView(ReportAuditIndexViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += CargarPantalla;
    }

    //Loaded se dispara cada vez que la pantalla vuelve a verse: la bandera evita recargar
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
