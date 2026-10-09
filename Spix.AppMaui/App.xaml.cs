using Spix.AppMaui.Services;

namespace Spix.AppMaui;

public partial class App : Application
{
    private readonly SessionService _session;

    public App(SessionService session)
    {
        InitializeComponent();

        _session = session;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();

        //Si el token guardado sigue vivo se entra derecho a la jornada: el tecnico no
        //tiene que escribir la clave cada manana
        shell.Dispatcher.Dispatch(async () =>
        {
            if (await _session.IsLoggedAsync())
            {
                await Shell.Current.GoToAsync("//today");
            }
        });

        return new Window(shell);
    }
}
