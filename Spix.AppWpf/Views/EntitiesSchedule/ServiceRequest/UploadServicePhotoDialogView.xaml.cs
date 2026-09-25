using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule.ServiceRequest;

// Sube una foto de la visita, del disco o de la camara.
public partial class UploadServicePhotoDialogView : UserControl, ISharedModalContent
{
    private readonly UploadServicePhotoDialogViewModel _viewModel;

    public UploadServicePhotoDialogView(UploadServicePhotoDialogViewModel viewModel)
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

    // La foto llega en el mismo Base64 venga del disco o de la camara
    private void PhotoSelected(object? sender, string base64)
    {
        _viewModel.SetPhoto(base64);
    }

    private void AntesElegido(object sender, RoutedEventArgs e)
    {
        _viewModel.IsAfter = false;
    }

    private void DespuesElegido(object sender, RoutedEventArgs e)
    {
        _viewModel.IsAfter = true;
    }
}
