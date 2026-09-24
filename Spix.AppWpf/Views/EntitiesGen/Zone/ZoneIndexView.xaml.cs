using Spix.AppWpf.ViewModels.EntitiesGen.Zone;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.Views.EntitiesGen.Zone;

// Muestra el indice paginado de zonas.
public partial class ZoneIndexView : UserControl
{
    private readonly ZoneIndexViewModel _viewModel;
    private bool _isLoaded;

    public ZoneIndexView(ZoneIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += LoadView;
    }

    private async void LoadView(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        Focus();

        await _viewModel.LoadAsync();
    }

    // Ctrl+F lleva el cursor al buscador
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchFilter.FocusSearch();
            e.Handled = true;
        }

        base.OnPreviewKeyDown(e);
    }
}
