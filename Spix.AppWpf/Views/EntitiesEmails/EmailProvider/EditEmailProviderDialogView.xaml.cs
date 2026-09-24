using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesEmails.EmailProvider;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesEmails.EmailProvider;

// Carga la configuracion recibida antes de permitir que el usuario la modifique.
public partial class EditEmailProviderDialogView : UserControl, ISharedModalContent
{
    private readonly EditEmailProviderDialogViewModel _viewModel;
    private Guid _providerId;
    private bool _isLoaded;

    public EditEmailProviderDialogView(EditEmailProviderDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null ||
            !parameters.TryGetValue("Id", out var id) ||
            id is not Guid providerId)
        {
            return;
        }

        _providerId = providerId;
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _providerId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadForEditAsync(_providerId);
    }
}
