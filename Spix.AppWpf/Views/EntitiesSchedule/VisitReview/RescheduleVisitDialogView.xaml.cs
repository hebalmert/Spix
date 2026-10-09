using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSchedule.VisitReview;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule.VisitReview;

// Reagendar la visita en la que no estaba el cliente.
public partial class RescheduleVisitDialogView : UserControl, ISharedModalContent
{
    private readonly RescheduleVisitDialogViewModel _viewModel;
    private Guid _id;
    private long _number;
    private string? _clientFullName;
    private bool _loaded;

    public RescheduleVisitDialogView(RescheduleVisitDialogViewModel viewModel)
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

        if (parameters.TryGetValue("ServiceRequestId", out var valor) && valor is Guid id)
        {
            _id = id;
        }

        if (parameters.TryGetValue("RequestNumber", out var numero) && numero is long number)
        {
            _number = number;
        }

        if (parameters.TryGetValue("ClientFullName", out var nombre) && nombre is string texto)
        {
            _clientFullName = texto;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync(_id, _number, _clientFullName);
    }
}
