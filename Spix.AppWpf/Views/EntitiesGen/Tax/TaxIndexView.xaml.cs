using Spix.AppWpf.ViewModels.EntitiesGen.Tax;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.Views.EntitiesGen.Tax;

// Muestra el indice paginado de impuestos.
public partial class TaxIndexView : UserControl
{
    private readonly TaxIndexViewModel _viewModel;
    private bool _isLoaded;

    public TaxIndexView(TaxIndexViewModel viewModel)
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
