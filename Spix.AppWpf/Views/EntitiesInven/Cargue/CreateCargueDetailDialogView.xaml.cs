using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Cargue;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Cargue;

// Vincula un serial nuevo con el cargue abierto desde el indice.
public partial class CreateCargueDetailDialogView : UserControl, ISharedModalContent
{
    private readonly CreateCargueDetailDialogViewModel _viewModel;
    private Guid _cargueId;
    private bool _isLoaded;

    public CreateCargueDetailDialogView(CreateCargueDetailDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("CargueId", out var value) == true && value is Guid cargueId)
        {
            _cargueId = cargueId;
        }
    }

    private void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _cargueId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        _viewModel.SetCargueId(_cargueId);
    }
}
