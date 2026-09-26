using Spix.AppWpf.ViewModels.EntitiesContratos.ContractSuspendedAudit;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractSuspendedAudit;

// Auditoria de contratos activados. Al abrir se consulta la ultima semana.
public partial class ContractSuspendedAuditIndexView : UserControl
{
    private readonly ContractSuspendedAuditIndexViewModel _viewModel;
    private bool _isLoaded;

    public ContractSuspendedAuditIndexView(ContractSuspendedAuditIndexViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += CargarPantalla;
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
