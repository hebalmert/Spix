using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesPayment.ContractExonerated;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.ContractExonerated;

// Nueva exoneracion. No recibe parametros: el contrato se busca aqui adentro.
public partial class CreateContractExoneratedDialogView : UserControl, ISharedModalContent
{
    private readonly CreateContractExoneratedDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateContractExoneratedDialogView(CreateContractExoneratedDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }

    //Los meses se bajan aqui y no en SetParameters: es una consulta al servidor
    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync();
    }
}
