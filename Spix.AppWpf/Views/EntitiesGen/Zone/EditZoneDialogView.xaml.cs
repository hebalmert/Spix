using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Zone;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Zone;

// Trae los estados y despues la zona, para que la ciudad quede seleccionada.
public partial class EditZoneDialogView : UserControl, ISharedModalContent
{
    private readonly EditZoneDialogViewModel _viewModel;
    private Guid _zoneId;
    private bool _isLoaded;

    public EditZoneDialogView(EditZoneDialogViewModel viewModel)
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
            id is not Guid zoneId)
        {
            return;
        }

        _zoneId = zoneId;
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _zoneId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;

        //Primero los estados, porque el combo necesita su lista para poder marcar el que
        //ya tiene la zona
        await _viewModel.InitializeAsync();
        await _viewModel.LoadForEditAsync(_zoneId);
    }
}
