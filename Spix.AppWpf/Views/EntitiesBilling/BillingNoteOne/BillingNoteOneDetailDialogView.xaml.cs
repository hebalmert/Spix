using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesBilling.BillingNoteOne;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesBilling.BillingNoteOne;

// El detalle de la nota individual, y el sitio donde se lanza.
//
// La carga va en el evento Loaded y no en SetParameters: SetParameters corre antes de que la
// ventana exista y una espera alli deja el modal en blanco.
public partial class BillingNoteOneDetailDialogView : UserControl, ISharedModalContent
{
    private readonly BillingNoteOneDetailDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public BillingNoteOneDetailDialogView(BillingNoteOneDetailDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += CargarPantalla;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null &&
            parameters.TryGetValue("Id", out var valor) &&
            valor is Guid id)
        {
            _id = id;
        }
    }

    private async void CargarPantalla(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync(_id);
    }
}
