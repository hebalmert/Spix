using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesPayment.ContractorCxC;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.ContractorCxC;

// Armar la cuenta. No recibe parametros: el contratista se elige aqui adentro.
public partial class CreateContractorCxCDialogView : UserControl, ISharedModalContent
{
    private readonly CreateContractorCxCDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateContractorCxCDialogView(CreateContractorCxCDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarPantalla;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
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
