namespace Spix.AppMaui.Services;

//La camara. La foto NO se guarda en el telefono: se lee a memoria, se manda en base64 y
//se acaba. Si el tecnico pierde el equipo, no se va con las fotos de los clientes.
public class PhotoService
{
    private readonly AlertService _alertService;

    public PhotoService(AlertService alertService)
    {
        _alertService = alertService;
    }

    public async Task<string?> TakeBase64Async()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await _alertService.WarningAsync("Foto", "Este telefono no tiene camara disponible.");
                return null;
            }

            var permiso = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permiso != PermissionStatus.Granted)
            {
                permiso = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (permiso != PermissionStatus.Granted)
            {
                await _alertService.WarningAsync("Foto", "La app necesita la camara para la foto del trabajo.");
                return null;
            }

            var archivo = await MediaPicker.Default.CapturePhotoAsync();
            if (archivo is null)
            {
                return null;
            }

            using var origen = await archivo.OpenReadAsync();
            using var memoria = new MemoryStream();
            await origen.CopyToAsync(memoria);

            return Convert.ToBase64String(memoria.ToArray());
        }
        catch (Exception)
        {
            await _alertService.WarningAsync("Foto", "No se pudo tomar la foto. Intenta de nuevo.");
            return null;
        }
    }
}
