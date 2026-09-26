using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractClient;

// Editar un contrato. Baja el contrato y rearma la cascada de estado, ciudad y zona.
public partial class EditContractClientDialogView : UserControl, ISharedModalContent
{
    private readonly EditContractClientDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public EditContractClientDialogView(EditContractClientDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null && parameters.TryGetValue("Id", out var valor) && valor is Guid id)
        {
            _id = id;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync(_id);
    }
}
