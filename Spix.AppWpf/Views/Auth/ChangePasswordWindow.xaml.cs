using Spix.AppWpf.ViewModels.Auth;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.Auth;

// Contiene el dialogo que solicita y confirma una nueva clave de acceso.
public partial class ChangePasswordWindow : Window
{
    private readonly ChangePasswordViewModel _viewModel;

    public ChangePasswordWindow(ChangePasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.PasswordChanged += CloseAfterPasswordChange;
        DataContext = _viewModel;
    }

    // Sincroniza la clave actual porque PasswordBox no permite enlace directo.
    private void CurrentPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentPassword = ((PasswordBox)sender).Password;
    }

    // Sincroniza la nueva clave sin almacenarla en controles de UI adicionales.
    private void NewPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.NewPassword = ((PasswordBox)sender).Password;
    }

    // Sincroniza la confirmacion para validarla antes de solicitar el cambio.
    private void ConfirmPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
    }

    // Permite leer el mensaje de exito antes de cerrar el dialogo.
    private async void CloseAfterPasswordChange(object? sender, EventArgs e)
    {
        await Task.Delay(900);
        Close();
    }

    // Cierra el dialogo sin enviar una modificacion al Backend.
    private void CancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // Desconecta eventos para evitar conservar el ViewModel luego del cierre.
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.PasswordChanged -= CloseAfterPasswordChange;
        base.OnClosed(e);
    }
}
