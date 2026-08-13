using Spix.AppWpf.ViewModels.EntitiesGen.DocumentType;
using System.Windows;
using System.Windows.Controls;

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
        await _viewModel.LoadAsync();
    }
}
