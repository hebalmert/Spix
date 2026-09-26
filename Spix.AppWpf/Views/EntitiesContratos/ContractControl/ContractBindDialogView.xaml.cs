using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using Spix.Domain.EntitiesContratos;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

// El IpBinding de acceso, armado con las piezas que ya quedaron configuradas.
public partial class ContractBindDialogView : UserControl, ISharedModalContent
{
    private readonly ContractBindDialogViewModel _viewModel;
    private Guid _id;
    private ContractServer? _server;
    private ContractIp? _ip;
    private ContractMac? _mac;
    private Guid _bindId;
    private bool _loaded;

    public ContractBindDialogView(ContractBindDialogViewModel viewModel)
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

        parameters.TryGetValue("Server", out var servidor);
        parameters.TryGetValue("Ip", out var ip);
        parameters.TryGetValue("Mac", out var mac);

        _server = servidor as ContractServer;
        _ip = ip as ContractIp;
        _mac = mac as ContractMac;

        //Con Id se esta editando: el IpBinding es la unica pieza que se cambia sin quitarla
        if (parameters.TryGetValue("Id", out var idBind) && idBind is Guid bindId)
        {
            _bindId = bindId;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;

        if (_bindId != Guid.Empty)
        {
            await _viewModel.InitializeEditAsync(_id, _bindId);
            return;
        }

        await _viewModel.InitializeAsync(_id, _server, _ip, _mac);
    }
}
