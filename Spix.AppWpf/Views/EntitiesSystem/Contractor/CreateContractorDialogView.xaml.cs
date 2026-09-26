using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSystem.Contractor;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Contractor;

// Carga el tipo de documento antes de presentar un contratista nuevo.
public partial class CreateContractorDialogView : UserControl, ISharedModalContent
{
    private readonly CreateContractorDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateContractorDialogView(CreateContractorDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += CargarDialogo;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }

    // La carga va en el Loaded y no en SetParameters: pedir datos antes de que el modal
    // este montado deja la pantalla en blanco.
    private async void CargarDialogo(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeForCreateAsync();
    }
}
