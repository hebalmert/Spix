using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppMaui.Services;

namespace Spix.AppMaui.ViewModels;

//Quien esta dentro del telefono y como salir
public partial class ProfileViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _userName = string.Empty;

    public string Version => AppInfo.Current.VersionString;

    public ProfileViewModel(SessionService session, AlertService alertService)
    {
        _session = session;
        _alertService = alertService;
    }

    public async Task LoadAsync()
    {
        await _session.GetTokenAsync();

        FullName = _session.FullName;
        UserName = _session.UserName;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirmado = await _alertService.ConfirmAsync("Salir", "Vas a cerrar la sesion.", "Salir");
        if (!confirmado)
        {
            return;
        }

        _session.Clear();

        await Shell.Current.GoToAsync("//login");
    }
}
