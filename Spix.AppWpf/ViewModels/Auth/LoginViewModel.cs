using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Auth;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppWpf.ViewModels.Auth;

// Mantiene el estado del formulario de acceso y delega la autenticacion al servicio.
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isProcessing;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public event EventHandler? LoginSucceeded;

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    // Actualiza la visibilidad del mensaje cuando cambia el resultado del acceso.
    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    // Impide solicitudes simultaneas mientras el Backend responde.
    partial void OnIsProcessingChanged(bool value)
    {
        LoginCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Ingresa tu usuario y clave.";
            return;
        }

        IsProcessing = true;

        var loginDTO = new LoginDTO
        {
            UserName = UserName.Trim(),
            Password = Password
        };

        var result = await _authenticationService.LoginAsync(loginDTO);

        IsProcessing = false;

        if (!result.WasSuccess)
        {
            ErrorMessage = result.Message ?? "No fue posible iniciar sesion.";
            return;
        }

        LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }

    private bool CanLogin()
    {
        return !IsProcessing;
    }
}
