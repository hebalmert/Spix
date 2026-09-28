using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

public partial class ContractPppoeDialogView : UserControl, ISharedModalContent
{
    private readonly ContractPppoeDialogViewModel _viewModel;
    private Guid _contractClientId;
    private bool _edit;
    private bool _loaded;

    public ContractPppoeDialogView(ContractPppoeDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters == null) return;
        if (parameters.TryGetValue("ContractClientId", out var id) && id is Guid contractId)
        {
            _contractClientId = contractId;
        }
        _edit = parameters.TryGetValue("Edit", out var edit) && edit is true;
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded) return;
        _loaded = true;
        await _viewModel.InitializeAsync(_contractClientId, _edit);
    }
}
