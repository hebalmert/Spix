using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using Spix.DomainLogic.EnumTypes;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

// Cambiar el estado del contrato.
public partial class ContractChangeStateDialogView : UserControl, ISharedModalContent
{
    private readonly ContractChangeStateDialogViewModel _viewModel;
    private Guid _id;
    private ContractState _currentState;
    private bool _loaded;

    public ContractChangeStateDialogView(ContractChangeStateDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null)
        {
            return;
        }

        if (parameters.TryGetValue("ContractClientId", out var valor) && valor is Guid id)
        {
            _id = id;
        }

        if (parameters.TryGetValue("CurrentState", out var estado) && estado is ContractState actual)
        {
            _currentState = actual;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync(_id, _currentState);
    }
}
