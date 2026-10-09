namespace Spix.AppMaui.Services;

//El GPS del telefono. Pide el permiso la primera vez y devuelve null si el tecnico lo
//niega o si no hay senal: quien llama decide que hacer, aqui no se inventa una posicion.
public class LocationService
{
    private readonly AlertService _alertService;

    public LocationService(AlertService alertService)
    {
        _alertService = alertService;
    }

    public async Task<Location?> GetAsync()
    {
        try
        {
            var permiso = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permiso != PermissionStatus.Granted)
            {
                permiso = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (permiso != PermissionStatus.Granted)
            {
                await _alertService.WarningAsync("Ubicacion",
                    "La app necesita el GPS para registrar donde estuviste.");
                return null;
            }

            //Alta precision: la diferencia que se mide es de 100 metros, no sirve lo aproximado
            var peticion = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(20));

            return await Geolocation.Default.GetLocationAsync(peticion)
                   ?? await Geolocation.Default.GetLastKnownLocationAsync();
        }
        catch (Exception)
        {
            await _alertService.WarningAsync("Ubicacion", "No se pudo leer el GPS. Intenta de nuevo.");
            return null;
        }
    }
}
