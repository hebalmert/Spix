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
    private bool _isBrowserReady;

    public SharedMapViewer()
    {
        InitializeComponent();
        Loaded += SharedMapViewerLoaded;
    }

    // Recibe el punto que se debe centrar y marcar en el mapa embebido.
    public async Task ShowMapAsync(decimal latitude, decimal longitude, string? title)
    {
        _latitude = latitude;
        _longitude = longitude;
        _title = string.IsNullOrWhiteSpace(title) ? "Ubicacion" : title;

        if (_isBrowserReady)
        {
            await RenderMapAsync();
        }
    }

    private async void SharedMapViewerLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await MapBrowser.EnsureCoreWebView2Async();
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
        MapBrowser.CoreWebView2.NavigateToString(CreateMapDocument());
        return Task.CompletedTask;
    }

    private string CreateMapDocument()
    {
        string latitude = _latitude!.Value.ToString(CultureInfo.InvariantCulture);
        string longitude = _longitude!.Value.ToString(CultureInfo.InvariantCulture);
        string title = JsonSerializer.Serialize(_title);

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
                </script>
            </body>
            </html>
            """;
    }
}
