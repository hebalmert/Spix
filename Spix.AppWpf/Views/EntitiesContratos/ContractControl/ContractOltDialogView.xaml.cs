using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

// La OLT de acceso de un contrato.
public partial class ContractOltDialogView : UserControl, ISharedModalContent
{
    private readonly ContractOltDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public ContractOltDialogView(ContractOltDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null && parameters.TryGetValue("ContractClientId", out var valor) && valor is Guid id)
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
