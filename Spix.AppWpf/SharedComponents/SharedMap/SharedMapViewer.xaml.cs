using Spix.AppWpf.SharedServices;
using Spix.xNetwork.MapHelper;
using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents.SharedMap;

// Presenta mapas de OpenStreetMap dentro de WPF para reutilizarlos en nodos, contratos y clientes.
public partial class SharedMapViewer : UserControl
{
    private decimal? _latitude;
    private decimal? _longitude;
    private string _title = "Ubicacion";

    //El segundo punto, opcional: sirve para ver al cliente y su nodo a la vez
    private decimal? _secondLatitude;
    private decimal? _secondLongitude;
    private string? _secondTitle;

    private bool _isBrowserReady;

    public SharedMapViewer()
    {
        InitializeComponent();
        Loaded += SharedMapViewerLoaded;
    }

    // Recibe el punto que se debe centrar y marcar en el mapa embebido.
    public Task ShowMapAsync(decimal latitude, decimal longitude, string? title)
    {
        return ShowMapAsync(latitude, longitude, title, null, null, null);
    }

    // Con un segundo punto se ven los dos y el mapa los encuadra a ambos: asi se aprecia
    // a que distancia esta el cliente de su nodo.
    public async Task ShowMapAsync(
        decimal latitude,
        decimal longitude,
        string? title,
        decimal? secondLatitude,
        decimal? secondLongitude,
        string? secondTitle)
    {
        _latitude = latitude;
        _longitude = longitude;
        _title = string.IsNullOrWhiteSpace(title) ? "Ubicacion" : title;

        _secondLatitude = secondLatitude;
        _secondLongitude = secondLongitude;
        _secondTitle = secondTitle;

        if (_isBrowserReady)
        {
            await RenderMapAsync();
        }
    }

    private async void SharedMapViewerLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await WebViewEnvironment.PrepararAsync(MapBrowser);
            _isBrowserReady = true;
            await RenderMapAsync();
        }
        catch (Exception exception)
        {
            MapErrorText.Text = $"No fue posible iniciar el visor del mapa. {exception.Message}";
            MapErrorText.Visibility = Visibility.Visible;
        }
    }

    private Task RenderMapAsync()
    {
        if (!_isBrowserReady || !_latitude.HasValue || !_longitude.HasValue)
        {
            return Task.CompletedTask;
        }

        MapErrorText.Visibility = Visibility.Collapsed;
        MapBrowser.CoreWebView2.Navigate(
            WebViewEnvironment.PublicarDocumento("ubicacion.html", CreateMapDocument()));
        return Task.CompletedTask;
    }

    private string CreateMapDocument()
    {
        string latitude = _latitude!.Value.ToString(CultureInfo.InvariantCulture);
        string longitude = _longitude!.Value.ToString(CultureInfo.InvariantCulture);
        string title = JsonSerializer.Serialize(_title);

        //Con segundo punto se pinta el otro marcador y se encuadran los dos
        var haySegundo = _secondLatitude.HasValue && _secondLongitude.HasValue;

        //La distancia en linea recta, con el MISMO calculo de la web (Spix.xNetwork):
        //no se recalcula aqui para que los dos lados digan siempre lo mismo.
        var distancia = haySegundo
            ? GeoDistance.Kilometers(_latitude.Value, _longitude.Value, _secondLatitude!.Value, _secondLongitude!.Value)
            : 0;

        var textoDistancia = JsonSerializer.Serialize($"{distancia:N2} Km");

        //Se usa $$ para que las llaves del JavaScript queden tal cual y la interpolacion
        //sea {{...}}: con un solo $ el compilador toma las llaves de JS como codigo.
        string segundo = haySegundo
            ? $$"""
                const second = [{{_secondLatitude!.Value.ToString(CultureInfo.InvariantCulture)}}, {{_secondLongitude!.Value.ToString(CultureInfo.InvariantCulture)}}];
                L.marker(second).addTo(map).bindPopup({{JsonSerializer.Serialize(_secondTitle ?? "Nodo")}});
                const line = L.polyline([point, second], { color: '#2563eb', weight: 4, opacity: 0.9 }).addTo(map);
                line.bindTooltip({{textoDistancia}}, { permanent: true, direction: 'center', className: 'spix-map-distance' }).openTooltip();
                map.fitBounds(line.getBounds(), { padding: [50, 50], maxZoom: 17 });
              """
            : string.Empty;

        return $$"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
                <style>
                    html, body, #map { width: 100%; height: 100%; margin: 0; background: #293847; }
                    .leaflet-control-layers { font-family: Segoe UI, Arial, sans-serif; }
                    .spix-map-distance {
                        background: rgba(255,255,255,.9);
                        border: 0; border-radius: 10px;
                        padding: 2px 8px; font-size: 12px; font-weight: 700; color: #16305e;
                        box-shadow: none;
                    }
                    .spix-map-distance::before { display: none; }
                </style>
            </head>
            <body>
                <div id="map"></div>
                <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
                <script>
                    const point = [{{latitude}}, {{longitude}}];
                    const title = {{title}};
                    const map = L.map('map', { scrollWheelZoom: true }).setView(point, 16);
                    const streetLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        maxZoom: 20,
                        attribution: '&copy; OpenStreetMap'
                    });
                    const satelliteLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
                        maxZoom: 20,
                        attribution: 'Tiles &copy; Esri'
                    });
                    streetLayer.addTo(map);
                    L.control.layers({ 'Mapa': streetLayer, 'Satelite': satelliteLayer }).addTo(map);
                    L.marker(point).addTo(map).bindPopup(title).openPopup();
                    {{segundo}}
                </script>
            </body>
            </html>
            """;
    }
}
