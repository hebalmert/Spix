using Microsoft.Web.WebView2.Core;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.AppWpf.SharedServices;
using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents.SharedMap;

// El mapa del nodo elegido: el AP, sus clientes, las lineas entre ellos y la mascara de
// cobertura del transmisor.
//
// Por dentro es el MISMO JavaScript de la web (wwwroot/jslib/nodeMap.js) corriendo en un
// WebView2: se porto tal cual para que el mapa se comporte igual en los dos lados y un
// arreglo alla se pueda traer aca sin traducir nada.
public partial class SharedNodeMapViewer : UserControl
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private bool _isReady;

    //Lo que se pidio antes de que el navegador estuviera listo, para lanzarlo al terminar.
    //Es una COLA y no una sola casilla: al elegir un nodo salen dos ordenes seguidas
    //(dibujar el mapa y pintar la mascara de cobertura). Con una sola casilla la segunda
    //pisaba a la primera, el dibujo nunca se ejecutaba y el mapa quedaba en blanco.
    private readonly Queue<string> _pendientes = new();

    public SharedNodeMapViewer()
    {
        InitializeComponent();
        Loaded += ViewerLoaded;
    }

    // Dibuja el nodo con sus clientes. Se llama cada vez que se cambia de nodo.
    public async Task RenderAsync(NodeMapDto data, int view)
    {
        var datos = JsonSerializer.Serialize(data, Json);

        await EjecutarAsync($"window.spixNodeMap.render('map', {datos}, {view});");
    }

    // Cambia la vista sin volver a pedir datos: solo redibuja las lineas.
    public async Task SetViewAsync(int view)
    {
        await EjecutarAsync($"window.spixNodeMap.setView('map', {view});");
    }

    // La mascara del transmisor: su ancho en grados y hacia donde apunta.
    public async Task SetCoverageAsync(int degrees, int azimuth, double radiusKm)
    {
        var radio = radiusKm.ToString(CultureInfo.InvariantCulture);

        await EjecutarAsync($"window.spixNodeMap.setCoverage('map', {degrees}, {azimuth}, {radio});");
    }

    private async void ViewerLoaded(object sender, RoutedEventArgs e)
    {
        if (_isReady)
        {
            return;
        }

        try
        {
            await WebViewEnvironment.PrepararAsync(MapBrowser);
            MapBrowser.CoreWebView2.NavigationCompleted += NavegacionTerminada;
            MapBrowser.CoreWebView2.Navigate(
                WebViewEnvironment.PublicarDocumento("nodo.html", Documento()));
        }
        catch (Exception exception)
        {
            MapErrorText.Text = $"No fue posible iniciar el visor del mapa. {exception.Message}";
            MapErrorText.Visibility = Visibility.Visible;
        }
    }

    private async void NavegacionTerminada(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        _isReady = true;

        //En el mismo orden en que se pidieron: primero dibujar, despues la cobertura
        while (_pendientes.Count > 0)
        {
            await MapBrowser.CoreWebView2.ExecuteScriptAsync(_pendientes.Dequeue());
        }
    }

    private async Task EjecutarAsync(string script)
    {
        if (!_isReady)
        {
            //Todavia carga el documento: se guarda en la cola y se lanza al terminar
            _pendientes.Enqueue(script);
            return;
        }

        await MapBrowser.CoreWebView2.ExecuteScriptAsync(script);
    }

    // El documento que vive dentro del navegador: Leaflet y el mismo JS de la web.
    private static string Documento()
    {
        return """
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
                        background: rgba(255,255,255,.85);
                        border: 0; border-radius: 10px;
                        padding: 1px 6px; font-size: 11px; font-weight: 600; color: #16305e;
                        box-shadow: none;
                    }
                    .spix-map-distance::before { display: none; }
                </style>
            </head>
            <body>
                <div id="map"></div>
                <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
                <script>
                window.spixNodeMap = window.spixNodeMap || {};

                const maps = {};

                // Pinta el AP y sus clientes como puntos, y las lineas segun la vista elegida
                // (1 = solo puntos, 2 = lineas al AP, 3 = lineas con la distancia encima)
                window.spixNodeMap.render = function (mapId, data, view) {
                    const element = document.getElementById(mapId);
                    if (!element || !window.L) { return; }

                    window.spixNodeMap.dispose(mapId);

                    const map = L.map(mapId, { scrollWheelZoom: true });

                    const streetLayer = L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                        maxZoom: 20, attribution: "&copy; OpenStreetMap"
                    });
                    const satelliteLayer = L.tileLayer("https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}", {
                        maxZoom: 20, attribution: "Tiles &copy; Esri"
                    });

                    streetLayer.addTo(map);
                    L.control.layers({ "Mapa": streetLayer, "Satelite": satelliteLayer }, null, { collapsed: false }).addTo(map);

                    const state = { map: map, node: null, clients: [], lines: L.layerGroup().addTo(map), coverage: L.layerGroup().addTo(map), bounds: [] };
                    maps[mapId] = state;

                    if (data.latitude !== null && data.longitude !== null) {
                        state.node = [Number(data.latitude), Number(data.longitude)];
                        L.circleMarker(state.node, { radius: 11, color: "#3c3489", weight: 3, fillColor: "#6f42c1", fillOpacity: 0.9 })
                            .addTo(map)
                            .bindPopup("<b>" + data.nodesName + "</b><br/>" + (data.ip || ""));
                        state.bounds.push(state.node);
                    }

                    (data.located || []).forEach(client => {
                        const point = [Number(client.latitude), Number(client.longitude)];
                        const distance = client.distanceKm !== null ? client.distanceKm.toFixed(2) + " Km" : null;
                        L.circleMarker(point, { radius: 7, color: "#27500a", weight: 2, fillColor: "#198754", fillOpacity: 0.85 })
                            .addTo(map)
                            .bindPopup("#" + client.controlContrato + " " + client.clientName + (distance ? " · " + distance : ""));
                        state.clients.push({ point: point, distance: distance });
                        state.bounds.push(point);
                    });

                    window.spixNodeMap.setView(mapId, view);
                    fitAll(state);
                };

                // Cambia la vista sin volver a pedir datos: solo se borran y redibujan las lineas
                window.spixNodeMap.setView = function (mapId, view) {
                    const state = maps[mapId];
                    if (!state) { return; }

                    state.lines.clearLayers();

                    // Sin coordenadas del AP no hay desde donde trazar
                    if (!state.node || Number(view) === 1) { return; }

                    const withDistance = Number(view) === 3;
                    state.clients.forEach(client => {
                        const line = L.polyline([state.node, client.point], { color: "#2563eb", weight: 2, opacity: 0.75 });
                        if (withDistance && client.distance) {
                            line.bindTooltip(client.distance, { permanent: true, direction: "center", className: "spix-map-distance" });
                        }
                        state.lines.addLayer(line);

                        // Las lineas van detras de los puntos, para que el clic en el cliente siga abriendo su popup
                        line.bringToBack();
                    });
                };

                // Suelta el mapa (al cambiar de nodo)
                window.spixNodeMap.dispose = function (mapId) {
                    const state = maps[mapId];
                    if (!state) { return; }

                    state.map.remove();
                    delete maps[mapId];
                };

                // Encuadra el AP y todos sus clientes
                function fitAll(state) {
                    if (state.bounds.length > 1) {
                        state.map.fitBounds(state.bounds, { padding: [40, 40], maxZoom: 17 });
                    } else if (state.bounds.length === 1) {
                        state.map.setView(state.bounds[0], 16);
                    } else {
                        state.map.setView([4.6, -74.08], 5);
                    }
                }

                // Mascara de cobertura: un sector con vertice en el AP, del ancho elegido y
                // girado hacia donde apunta el transmisor. Con 0 grados no se pinta nada.
                window.spixNodeMap.setCoverage = function (mapId, degrees, azimuth, radiusKm) {
                    const state = maps[mapId];
                    if (!state) { return; }

                    state.coverage.clearLayers();

                    if (!state.node || Number(degrees) <= 0) { return; }

                    const width = Number(degrees);
                    const start = Number(azimuth) - (width / 2);
                    const points = [state.node];

                    // Un punto por grado: el borde del sector queda parejo
                    for (let i = 0; i <= width; i++) {
                        points.push(destination(state.node, start + i, Number(radiusKm)));
                    }
                    points.push(state.node);

                    const sector = L.polygon(points, {
                        color: "#6f42c1", weight: 2, opacity: 0.9, fillColor: "#6f42c1", fillOpacity: 0.12
                    });

                    state.coverage.addLayer(sector);

                    // El sector va detras de todo, para no tapar los puntos ni las lineas
                    sector.bringToBack();
                };

                // Punto a "distanceKm" del origen, en el rumbo indicado (0 = norte, 90 = este)
                function destination(origin, bearingDeg, distanceKm) {
                    const radius = 6371.0088;
                    const angular = distanceKm / radius;
                    const bearing = bearingDeg * Math.PI / 180;
                    const lat1 = origin[0] * Math.PI / 180;
                    const lng1 = origin[1] * Math.PI / 180;

                    const lat2 = Math.asin((Math.sin(lat1) * Math.cos(angular)) +
                        (Math.cos(lat1) * Math.sin(angular) * Math.cos(bearing)));

                    const lng2 = lng1 + Math.atan2(
                        Math.sin(bearing) * Math.sin(angular) * Math.cos(lat1),
                        Math.cos(angular) - (Math.sin(lat1) * Math.sin(lat2)));

                    return [lat2 * 180 / Math.PI, lng2 * 180 / Math.PI];
                }
                </script>
            </body>
            </html>
            """;
    }
}
