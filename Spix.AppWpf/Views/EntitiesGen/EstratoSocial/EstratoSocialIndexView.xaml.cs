using Spix.AppWpf.ViewModels.EntitiesGen.EstratoSocial;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.EstratoSocial;

// Muestra el indice paginado de estratos sociales.
public partial class EstratoSocialIndexView : UserControl
{
    private readonly EstratoSocialIndexViewModel _viewModel;
    private bool _isLoaded;

    public EstratoSocialIndexView(EstratoSocialIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += LoadView;
    }

    // Solicita registros solo la primera vez que la vista entra al contenedor principal.
    private async void LoadView(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync();
    }
}
