using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractClient;

// Las dos caras del documento del cliente.
public partial class ContractIdPicsDialogView : UserControl, ISharedModalContent
{
    private readonly ContractIdPicsDialogViewModel _viewModel;
    private Guid _contractClientId;
    private Guid? _idPics;
    private bool _loaded;

    public ContractIdPicsDialogView(ContractIdPicsDialogViewModel viewModel)
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

        if (parameters.TryGetValue("ContractClientId", out var contrato) && contrato is Guid id)
        {
            _contractClientId = id;
        }

        //Solo llega cuando el contrato YA tiene fotos: entonces se editan
        if (parameters.TryGetValue("Id", out var fotos) && fotos is Guid idPics)
        {
            _idPics = idPics;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync(_contractClientId, _idPics);
    }

    private void FrontSelected(object? sender, string base64)
    {
        _viewModel.SetFront(base64);
    }

    private void BackSelected(object? sender, string base64)
    {
        _viewModel.SetBack(base64);
    }
}
