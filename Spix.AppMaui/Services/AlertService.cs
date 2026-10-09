namespace Spix.AppMaui.Services;

//Los avisos en un solo sitio: la pagina no llama a DisplayAlert por su cuenta
public class AlertService
{
    public Task InfoAsync(string titulo, string mensaje) => MostrarAsync(titulo, mensaje);

    public Task WarningAsync(string titulo, string mensaje) => MostrarAsync(titulo, mensaje);

    public Task ErrorAsync(string mensaje) => MostrarAsync("Error", mensaje);

    public async Task<bool> ConfirmAsync(string titulo, string mensaje, string aceptar)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
        {
            return false;
        }

        return await page.DisplayAlert(titulo, mensaje, aceptar, "Cancelar");
    }

    private static async Task MostrarAsync(string titulo, string mensaje)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
        {
            return;
        }

        await page.DisplayAlert(titulo, mensaje, "Aceptar");
    }
}
