using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.Node;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Node;

// Recibe las coordenadas del nodo seleccionado para presentarlas en el visor de mapa.
public partial class NodeMapDialogView : UserControl, ISharedModalContent
{
    private readonly NodeMapDialogViewModel _viewModel;

    public NodeMapDialogView(NodeMapDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Latitude", out var latitudeValue) != true ||
            parameters.TryGetValue("Longitude", out var longitudeValue) != true ||
            latitudeValue is not decimal latitude ||
            longitudeValue is not decimal longitude)
        {
            return;
        }

        parameters.TryGetValue("Title", out var titleValue);
        _viewModel.SetMap(latitude, longitude, titleValue?.ToString());
        _ = MapViewer.ShowMapAsync(latitude, longitude, titleValue?.ToString());
    }
}
