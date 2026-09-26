using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Usuario;

// El cascaron del modal para editar: baja el usuario seleccionado antes de mostrarlo.
public partial class EditUsuarioDialogView : UserControl, ISharedModalContent
{
    private readonly EditUsuarioDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditUsuarioDialogView(EditUsuarioDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarModal;
    }

    // Solo se guarda el id: la consulta se hace en Loaded, no aqui
    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Id", out var value) == true && value is Guid id)
        {
            _id = id;
        }
    }

    private async void CargarModal(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync(_id);
    }
}
