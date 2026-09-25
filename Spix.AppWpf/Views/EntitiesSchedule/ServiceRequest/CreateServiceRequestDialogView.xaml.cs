using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule.ServiceRequest;

// Registrar una solicitud de servicio. Al abrir se baja la lista de tecnicos.
public partial class CreateServiceRequestDialogView : UserControl, ISharedModalContent
{
    private readonly CreateServiceRequestDialogViewModel _viewModel;
    private bool _loaded;

    public CreateServiceRequestDialogView(CreateServiceRequestDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync();
    }
}
