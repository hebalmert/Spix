using Spix.AppWpf.ViewModels.EntitiesSchedule.VisitReview;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule.VisitReview;

// Bandeja de revision. Al abrir se piden los contadores y la primera pagina.
public partial class VisitReviewIndexView : UserControl
{
    private readonly VisitReviewIndexViewModel _viewModel;
    private bool _isLoaded;

    public VisitReviewIndexView(VisitReviewIndexViewModel viewModel)
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
