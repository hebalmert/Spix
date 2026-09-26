using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesPayment.ContractExonerated;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.ContractExonerated;

// Editar una exoneracion. Baja los meses y el registro guardado.
public partial class EditContractExoneratedDialogView : UserControl, ISharedModalContent
{
    private readonly EditContractExoneratedDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditContractExoneratedDialogView(EditContractExoneratedDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null && parameters.TryGetValue("Id", out var valor) && valor is Guid id)
        {
            _id = id;
        }
    }

    //Las consultas van en el Loaded, no en SetParameters
    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync(_id);
    }
}
