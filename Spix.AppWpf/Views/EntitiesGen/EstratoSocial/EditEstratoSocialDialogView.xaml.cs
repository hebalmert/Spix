using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.EstratoSocial;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.EstratoSocial;

// Carga el estrato recibido antes de permitir su edicion.
public partial class EditEstratoSocialDialogView : UserControl, ISharedModalContent
{
    private readonly EditEstratoSocialDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditEstratoSocialDialogView(EditEstratoSocialDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Id", out var value) == true && value is Guid id)
        {
            _id = id;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty) return;
        _isLoaded = true;
        await _viewModel.LoadAsync(_id);
    }
}
