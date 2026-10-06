using Microsoft.Web.WebView2.Core;
using Spix.AppWpf.SharedServices;
using Spix.DomainLogic.EntitiesNetDTO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents.SharedMap;

// El mapa de OLT: todas las OLT con su nombre, o una sola con sus clientes y las lineas.
//
// Por dentro es el MISMO JavaScript de la web (wwwroot/jslib/oltMap.js) corriendo en un
// WebView2: se porto tal cual para que el mapa se comporte igual en los dos lados y un
// arreglo alla se pueda traer aca sin traducir nada.
//
// No lleva mascara de cobertura: eso es el sector de un transmisor inalambrico y en fibra
// no significa nada. Por eso es un visor aparte del de nodos y no una variante suya.
public partial class SharedOltMapViewer : UserControl
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private bool _isReady;

    //Lo que se pidio antes de que el navegador estuviera listo. Es una COLA y no una sola
    //casilla: con una sola, la segunda orden pisaba a la primera y el mapa quedaba en blanco.
    private readonly Queue<string> _pendientes = new();

    public SharedOltMapViewer()
    {
        InitializeComponent();
        Loaded += ViewerLoaded;
    }

    // Todas las OLT con su nombre y cuantos clientes tiene cada una.
    public async Task RenderAllAsync(IEnumerable<OltMapItemDto> olts, string clientsLabel)
    {
        var datos = JsonSerializer.Serialize(olts, Json);
        var etiqueta = JsonSerializer.Serialize(clientsLabel, Json);

        await EjecutarAsync($"window.spixOltMap.renderAll('map', {datos}, {etiqueta});");
    }

    // Una OLT con sus clientes. Se llama cada vez que se cambia de OLT.
    public async Task RenderAsync(OltMapDto data, int view)
    {
        var datos = JsonSerializer.Serialize(data, Json);

        await EjecutarAsync($"window.spixOltMap.render('map', {datos}, {view});");
    }

    // Cambia la vista sin volver a pedir datos: solo redibuja las lineas.
    public async Task SetViewAsync(int view)
    {
        await EjecutarAsync($"window.spixOltMap.setView('map', {view});");
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
                WebViewEnvironment.PublicarDocumento("olt.html", Documento()));
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

        //En el mismo orden en que se pidieron
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
                    .spix-map-distance, .spix-map-oltname {
                        background: rgba(255,255,255,.85);
                        border: 0; border-radius: 10px;
                        padding: 1px 6px; font-size: 11px; font-weight: 600; color: #16305e;
                        box-shadow: none;
                    }
                    .spix-map-distance::before, .spix-map-oltname::before { display: none; }
                </style>
            </head>
            <body>
                <div id="map"></div>
                <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
                <script>
                window.spixOltMap = window.spixOltMap || {};

                const oltMaps = {};

                // El mapa en blanco con sus dos capas, comun a las dos vistas
                function crear(mapId) {
                    const element = document.getElementById(mapId);
                    if (!element || !window.L) { return null; }

                    window.spixOltMap.dispose(mapId);

                    const map = L.map(mapId, { scrollWheelZoom: true });

                    const streetLayer = L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                        maxZoom: 20, attribution: "&copy; OpenStreetMap"
                    });
                    const satelliteLayer = L.tileLayer("https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}", {
                        maxZoom: 20, attribution: "Tiles &copy; Esri"
                    });

                    streetLayer.addTo(map);
                    L.control.layers({ "Mapa": streetLayer, "Satelite": satelliteLayer }, null, { collapsed: false }).addTo(map);

                    const state = { map: map, olt: null, clients: [], lines: L.layerGroup().addTo(map), bounds: [] };
                    oltMaps[mapId] = state;

                    return state;
                }

                // El punto de una OLT: siempre el mismo simbolo, en la vista de una y en la de todas
                function marcarOlt(state, point, popup) {
                    L.circleMarker(point, { radius: 11, color: "#3c3489", weight: 3, fillColor: "#6f42c1", fillOpacity: 0.9 })
                        .addTo(state.map)
                        .bindPopup(popup);
                    state.bounds.push(point);
                }

                // Vista de TODAS: cada OLT con su nombre siempre visible y cuantos clientes tiene.
                window.spixOltMap.renderAll = function (mapId, olts, clientsLabel) {
                    const state = crear(mapId);
                    if (!state) { return; }

                    (olts || []).forEach(olt => {
                        if (olt.latitude === null || olt.longitude === null) { return; }

                        const point = [Number(olt.latitude), Number(olt.longitude)];
                        marcarOlt(state, point, "<b>" + olt.oltName + "</b><br/>" + (olt.ip || "") +
                            "<br/>" + olt.clients + " " + clientsLabel);

                        // El nombre queda escrito en el mapa: se busca la OLT de un vistazo
                        L.marker(point, { opacity: 0 })
                            .addTo(state.map)
                            .bindTooltip(olt.oltName + " · " + olt.clients, {
                                permanent: true, direction: "top", offset: [0, -10], className: "spix-map-oltname"
                            });
                    });

                    encuadrar(state);
                };

                // Vista de UNA: la OLT y sus clientes como puntos, y las lineas segun la vista
                // (1 = solo puntos, 2 = lineas a la OLT, 3 = lineas con la distancia encima)
                window.spixOltMap.render = function (mapId, data, view) {
                    const state = crear(mapId);
                    if (!state) { return; }

                    if (data.latitude !== null && data.longitude !== null) {
                        state.olt = [Number(data.latitude), Number(data.longitude)];
                        marcarOlt(state, state.olt, "<b>" + data.oltName + "</b><br/>" + (data.ip || ""));
                    }

                    (data.located || []).forEach(client => {
                        const point = [Number(client.latitude), Number(client.longitude)];
                        const distance = client.distanceKm !== null ? client.distanceKm.toFixed(2) + " Km" : null;
                        L.circleMarker(point, { radius: 7, color: "#27500a", weight: 2, fillColor: "#198754", fillOpacity: 0.85 })
                            .addTo(state.map)
                            .bindPopup("#" + client.controlContrato + " " + client.clientName + (distance ? " · " + distance : ""));
                        state.clients.push({ point: point, distance: distance });
                        state.bounds.push(point);
                    });

                    window.spixOltMap.setView(mapId, view);
                    encuadrar(state);
                };

                // Cambia la vista sin volver a pedir datos: solo se borran y redibujan las lineas
                window.spixOltMap.setView = function (mapId, view) {
                    const state = oltMaps[mapId];
                    if (!state) { return; }

                    state.lines.clearLayers();

                    // Sin coordenadas de la OLT no hay desde donde trazar
                    if (!state.olt || Number(view) === 1) { return; }

                    const withDistance = Number(view) === 3;
                    state.clients.forEach(client => {
                        const line = L.polyline([state.olt, client.point], { color: "#2563eb", weight: 2, opacity: 0.75 });
                        if (withDistance && client.distance) {
                            line.bindTooltip(client.distance, { permanent: true, direction: "center", className: "spix-map-distance" });
                        }
                        state.lines.addLayer(line);

                        // Las lineas van detras de los puntos, para que el clic siga abriendo su popup
                        line.bringToBack();
                    });
                };

                // Suelta el mapa (al cambiar de OLT)
                window.spixOltMap.dispose = function (mapId) {
                    const state = oltMaps[mapId];
                    if (!state) { return; }

                    state.map.remove();
                    delete oltMaps[mapId];
                };

                // Encuadra todo lo que se pinto
                function encuadrar(state) {
                    if (state.bounds.length > 1) {
                        state.map.fitBounds(state.bounds, { padding: [40, 40], maxZoom: 17 });
                    } else if (state.bounds.length === 1) {
                        state.map.setView(state.bounds[0], 16);
                    } else {
                        state.map.setView([4.6, -74.08], 5);
                    }
                }
                </script>
            </body>
            </html>
            """;
    }
}
