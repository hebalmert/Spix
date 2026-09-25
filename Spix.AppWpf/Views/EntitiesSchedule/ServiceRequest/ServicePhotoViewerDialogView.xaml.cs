using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;
using Spix.Domain.EntitiesSchedule;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule.ServiceRequest;

// Carrusel de las fotos de la visita.
public partial class ServicePhotoViewerDialogView : UserControl, ISharedModalContent
{
    private readonly ServicePhotoViewerDialogViewModel _viewModel;

    public ServicePhotoViewerDialogView(ServicePhotoViewerDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null)
        {
            return;
        }

        var fotos = parameters.TryGetValue("Photos", out var lista) && lista is List<ServiceRequestPhotoDto> valores
            ? valores
            : new List<ServiceRequestPhotoDto>();

        var inicial = parameters.TryGetValue("StartId", out var id) && id is Guid guid
            ? guid
            : Guid.Empty;

        _viewModel.Initialize(fotos, inicial);
    }
}
