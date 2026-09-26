using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using Spix.Domain.EntitiesContratos;
using System.Windows;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

// La Queue de velocidad, armada con las piezas que ya quedaron configuradas.
public partial class ContractQueueDialogView : UserControl, ISharedModalContent
{
    private readonly ContractQueueDialogViewModel _viewModel;
    private Guid _id;
    private ContractServer? _server;
    private ContractIp? _ip;
    private ContractPlan? _plan;
    private bool _loaded;

    public ContractQueueDialogView(ContractQueueDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null ||
            !parameters.TryGetValue("ContractClientId", out var valor) ||
            valor is not Guid id)
        {
            return;
        }

        parameters.TryGetValue("Server", out var servidor);
        parameters.TryGetValue("Ip", out var ip);
        parameters.TryGetValue("Plan", out var plan);

        _id = id;
        _server = servidor as ContractServer;
        _ip = ip as ContractIp;
        _plan = plan as ContractPlan;
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync(_id, _server, _ip, _plan);
    }
}
