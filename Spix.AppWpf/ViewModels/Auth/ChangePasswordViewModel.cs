using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Auth;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppWpf.ViewModels.Auth;

// Gestiona la validacion y actualizacion de clave sin acoplarla a la ventana.
public partial class ChangePasswordViewModel : ObservableObject
{
    private readonly IAccountService _accountService;

    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _wasSuccess;

    [ObservableProperty]
    private bool _isProcessing;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public event EventHandler? PasswordChanged;

    public ChangePasswordViewModel(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // Actualiza el aviso visible cuando cambia la respuesta del Backend.
    partial void OnMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasMessage));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        Message = string.Empty;
        WasSuccess = false;

        if (string.IsNullOrWhiteSpace(CurrentPassword) ||
            string.IsNullOrWhiteSpace(NewPassword) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            Message = "Completa los tres campos de clave.";
            return;
        }

        if (NewPassword.Length < 6 || NewPassword.Length > 20)
        {
            Message = "La nueva clave debe tener entre 6 y 20 caracteres.";
            return;
        }

        if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
        {
            Message = "La confirmacion no coincide con la nueva clave.";
            return;
        }

        IsProcessing = true;

        var result = await _accountService.ChangePasswordAsync(new ChangePasswordDTO
        {
            CurrentPassword = CurrentPassword,
            NewPassword = NewPassword,
            Confirm = ConfirmPassword
        });

        IsProcessing = false;
        WasSuccess = result.WasSuccess;
        Message = result.Message ?? "No fue posible actualizar la clave.";

        if (result.WasSuccess)
        {
            PasswordChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
