using Microsoft.Extensions.DependencyInjection;
using Spix.AppWpf.ViewModels.Auth;
using System.Windows;

namespace Spix.AppWpf.Views.Auth;

// Contiene el formulario visual de acceso antes de conectar la autenticación con el Backend.
public partial class LoginWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LoginViewModel _viewModel;
    private bool _isPasswordVisible;

    public LoginWindow(
        LoginViewModel viewModel,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        _viewModel.LoginSucceeded += OpenMainWindow;
        DataContext = _viewModel;
    }

    // Alterna entre los controles para permitir revisar la contraseña escrita.
    private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;

        if (_isPasswordVisible)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
            OpenEyeIcon.Visibility = Visibility.Collapsed;
            ClosedEyeIcon.Visibility = Visibility.Visible;
            TogglePasswordButton.ToolTip = "Ocultar contraseña";
            PasswordTextBox.Focus();
            PasswordTextBox.CaretIndex = PasswordTextBox.Text.Length;
            return;
        }

        PasswordBox.Password = PasswordTextBox.Text;
        PasswordTextBox.Visibility = Visibility.Collapsed;
        PasswordBox.Visibility = Visibility.Visible;
        ClosedEyeIcon.Visibility = Visibility.Collapsed;
        OpenEyeIcon.Visibility = Visibility.Visible;
        TogglePasswordButton.ToolTip = "Mostrar contraseña";
        PasswordBox.Focus();
    }

    // Sincroniza el PasswordBox porque WPF no permite enlazar su clave directamente.
    private void PasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = PasswordBox.Password;
    }

    // Abre el cascaron principal despues de recibir un JWT valido del Backend.
    private void OpenMainWindow(object? sender, EventArgs e)
    {
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        Close();
    }

    // Desconecta eventos de la ventana que ya no necesita recibir cambios de sesion.
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LoginSucceeded -= OpenMainWindow;
        base.OnClosed(e);
    }
}
