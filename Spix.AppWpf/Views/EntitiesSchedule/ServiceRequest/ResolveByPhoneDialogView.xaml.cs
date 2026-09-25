using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule.ServiceRequest;

// Cierra la solicitud sin visita.
public partial class ResolveByPhoneDialogView : UserControl, ISharedModalContent
{
    private readonly ResolveByPhoneDialogViewModel _viewModel;

    public ResolveByPhoneDialogView(ResolveByPhoneDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null && parameters.TryGetValue("ServiceRequestId", out var valor) && valor is Guid id)
        {
            _viewModel.Initialize(id);
        }
    }
}
