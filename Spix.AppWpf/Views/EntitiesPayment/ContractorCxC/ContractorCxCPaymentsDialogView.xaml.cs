using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesPayment.ContractorCxC;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.ContractorCxC;

// Los abonos que se le han hecho a la cuenta.
public partial class ContractorCxCPaymentsDialogView : UserControl, ISharedModalContent
{
    private readonly ContractorCxCPaymentsDialogViewModel _viewModel;
    private bool _isLoaded;

    public ContractorCxCPaymentsDialogView(ContractorCxCPaymentsDialogViewModel viewModel)
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
