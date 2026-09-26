using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesPayment.ContractorCxC;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.ContractorCxC;

// Las comisiones que se agruparon en la cuenta.
public partial class ContractorCxCCommissionsDialogView : UserControl, ISharedModalContent
{
    private readonly ContractorCxCCommissionsDialogViewModel _viewModel;
    private bool _isLoaded;

    public ContractorCxCCommissionsDialogView(ContractorCxCCommissionsDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarPantalla;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null &&
            parameters.TryGetValue("Id", out var valor) &&
            valor is Guid id)
        {
            _viewModel.SetId(id);
        }
    }

    private async void CargarPantalla(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync();
    }
}
