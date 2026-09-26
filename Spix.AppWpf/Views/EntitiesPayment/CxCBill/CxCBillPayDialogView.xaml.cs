using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesPayment.CxCBill;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.CxCBill;

// Recibir el dinero de una nota. Carga en el Loaded, cuando el Id ya llego.
public partial class CxCBillPayDialogView : UserControl, ISharedModalContent
{
    private readonly CxCBillPayDialogViewModel _viewModel;
    private bool _isLoaded;

    public CxCBillPayDialogView(CxCBillPayDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarPantalla;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null &&
            parameters.TryGetValue("Id", out var valor) &&
            valor is Guid id)
        {
            _viewModel.SetId(id);
        }
    }

    private async void CargarPantalla(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync();
    }
}
