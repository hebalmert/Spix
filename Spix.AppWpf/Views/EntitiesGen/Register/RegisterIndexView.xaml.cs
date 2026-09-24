using Spix.AppWpf.ViewModels.EntitiesGen.Register;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.Views.EntitiesGen.Register;

// Muestra el indice paginado de los consecutivos.
public partial class RegisterIndexView : UserControl
{
    private readonly RegisterIndexViewModel _viewModel;
    private bool _isLoaded;

    public RegisterIndexView(RegisterIndexViewModel viewModel)
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
