using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSystem.Contractor;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Contractor;

// Recupera el contratista individual antes de permitir modificar sus datos.
public partial class EditContractorDialogView : UserControl, ISharedModalContent
{
    private readonly EditContractorDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditContractorDialogView(EditContractorDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += CargarDialogo;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters != null && parameters.TryGetValue("Id", out object? value) && value is Guid id)
        {
            _id = id;
        }
    }

    // La carga va en el Loaded y no en SetParameters: pedir datos antes de que el modal
    // este montado deja la pantalla en blanco.
    private async void CargarDialogo(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeForEditAsync(_id);
    }
}
