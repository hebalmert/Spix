using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesPayment.PrePayment;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.PrePayment;

// Registrar o corregir un pago adelantado. Sin parametro Id se crea uno nuevo.
public partial class PrePaymentDialogView : UserControl, ISharedModalContent
{
    private readonly PrePaymentDialogViewModel _viewModel;
    private Guid _prePaymentId;
    private bool _isLoaded;

    public PrePaymentDialogView(PrePaymentDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += CargarModal;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null ||
            !parameters.TryGetValue("Id", out var valor) ||
            valor is not Guid prePaymentId)
        {
            return;
        }

        _prePaymentId = prePaymentId;
    }

    // Lo que se baja del servidor va aqui y no en SetParameters: alli el modal todavia no
    // se esta viendo y el spinner no se pintaria
    private async void CargarModal(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync(_prePaymentId);
    }
}
