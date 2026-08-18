using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSchedule;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule;

// Muestra en solo lectura los datos que provienen de una solicitud de servicio.
public partial class ServiceRequestScheduleInfoDialogView : UserControl, ISharedModalContent
{
    private readonly ServiceRequestScheduleInfoDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public ServiceRequestScheduleInfoDialogView(ServiceRequestScheduleInfoDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters != null && parameters.TryGetValue("Id", out object? value) && value is Guid id)
        {
            _id = id;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded || _id == Guid.Empty)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync(_id);
    }
}
