using Spix.AppWpf.ViewModels.EntitiesGen.DocumentType;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.Views.EntitiesGen.DocumentType;

// Muestra el indice paginado de tipos de documento.
public partial class DocumentTypeIndexView : UserControl
{
    private readonly DocumentTypeIndexViewModel _viewModel;
    private bool _isLoaded;

    public DocumentTypeIndexView(DocumentTypeIndexViewModel viewModel)
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

        //El teclado empieza en la vista: asi Ctrl+N y F5 responden sin hacer clic antes
        Focus();

        await _viewModel.LoadAsync();
    }

    // Ctrl+F lleva el cursor al buscador. El foco es cosa de la pantalla, no del ViewModel.
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
