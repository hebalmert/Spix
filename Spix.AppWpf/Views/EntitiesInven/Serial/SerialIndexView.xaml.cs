using Spix.AppWpf.ViewModels.EntitiesInven.Serial;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Serial;

// Carga las MAC paginadas cuando el usuario abre el modulo Cargue Seriales.
public partial class SerialIndexView : UserControl
{
    private readonly SerialIndexViewModel _viewModel;
    private bool _isLoaded;

    public SerialIndexView(SerialIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    private async void LoadView(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync();
    }
}
