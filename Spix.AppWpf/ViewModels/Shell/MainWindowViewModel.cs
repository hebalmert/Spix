using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Session;

namespace Spix.AppWpf.ViewModels.Shell;

// Expone la informacion de sesion y las acciones globales del escritorio.
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IUserSessionService _sessionService;

    [ObservableProperty]
    private bool _isAccountMenuOpen;

    public string UserName => _sessionService.UserName;

    public string FullName => _sessionService.FullName;

    public string Role => _sessionService.Role;

    public string CorporationName => _sessionService.CorporationName;

    public string Initials => _sessionService.Initials;

    public event EventHandler? ChangePasswordRequested;

    public event EventHandler? LogoutRequested;

    public MainWindowViewModel(IUserSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    // Abre o cierra las acciones de la sesion sin abandonar la pantalla actual.
    [RelayCommand]
    private void ToggleAccountMenu()
    {
        IsAccountMenuOpen = !IsAccountMenuOpen;
    }

    // Solicita el formulario seguro que actualiza la clave contra el Backend.
    [RelayCommand]
    private void ChangePassword()
    {
        IsAccountMenuOpen = false;
        ChangePasswordRequested?.Invoke(this, EventArgs.Empty);
    }

    // Libera la sesion antes de devolver el usuario al formulario de acceso.
    [RelayCommand]
    private void Logout()
    {
        IsAccountMenuOpen = false;
        _sessionService.ClearSession();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}
