using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

// La ubicacion del servicio.
public partial class ContractMapDialogView : UserControl, ISharedModalContent
{
    private readonly ContractMapDialogViewModel _viewModel;
    private Guid _id;
    private Guid? _mapId;
    private bool _loaded;

    public ContractMapDialogView(ContractMapDialogViewModel viewModel)
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

        //Solo llega cuando la ubicacion YA existe: entonces se edita
        if (parameters.TryGetValue("Id", out var mapa) && mapa is Guid mapId)
        {
            _mapId = mapId;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync(_id, _mapId);
    }
}
