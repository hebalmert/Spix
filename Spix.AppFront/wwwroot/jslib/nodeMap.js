// Mapa de nodos: JS propio de esta pantalla.
// Va aparte de spixMap.js (mapa del contrato y de Nodos): no comparten nada, asi un cambio aqui
// no los toca. Solo se ejecuta cuando se abre /nodemap.
// Leaflet (window.L) ya viene cargado por index.html.

window.spixNodeMap = window.spixNodeMap || {};

const maps = {};

// Pinta el AP y sus clientes como puntos, y las lineas segun la vista elegida
// (1 = solo puntos, 2 = lineas al AP, 3 = lineas con la distancia encima)
window.spixNodeMap.render = function (mapId, data, view) {
    const element = document.getElementById(mapId);
    if (!element || !window.L) {
        return;
    }

    window.spixNodeMap.dispose(mapId);

    const map = L.map(mapId, { scrollWheelZoom: true });

    const streetLayer = L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 20,
        attribution: "&copy; OpenStreetMap"
    });

    const satelliteLayer = L.tileLayer("https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}", {
        maxZoom: 20,
        attribution: "Tiles &copy; Esri"
    });

    streetLayer.addTo(map);
    L.control.layers({
        "Mapa": streetLayer,
        "Satelite": satelliteLayer
    }, null, { collapsed: false }).addTo(map);

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
    if (!state) {
        return;
    }

    state.lines.clearLayers();

    // Sin coordenadas del AP no hay desde donde trazar
    if (!state.node || Number(view) === 1) {
        return;
    }

    const withDistance = Number(view) === 3;
    state.clients.forEach(client => {
        const line = L.polyline([state.node, client.point], { color: "#2563eb", weight: 2, opacity: 0.75 });
        if (withDistance && client.distance) {
            line.bindTooltip(client.distance, {
                permanent: true,
                direction: "center",
                className: "spix-map-distance"
            });
        }
        state.lines.addLayer(line);

        // Las lineas van detras de los puntos, para que el clic en el cliente siga abriendo su popup
        line.bringToBack();
    });
};

// Suelta el mapa (al cambiar de nodo o al salir de la pantalla)
window.spixNodeMap.dispose = function (mapId) {
    const state = maps[mapId];
    if (!state) {
        return;
    }

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

// Mascara de cobertura: un sector con vertice en el AP, del ancho elegido (45 o 90 grados)
// y girado hacia donde apunta el transmisor (azimut). Con 0 grados no se pinta nada.
window.spixNodeMap.setCoverage = function (mapId, degrees, azimuth, radiusKm) {
    const state = maps[mapId];
    if (!state) {
        return;
    }

    state.coverage.clearLayers();

    if (!state.node || Number(degrees) <= 0) {
        return;
    }

    const width = Number(degrees);
    const start = Number(azimuth) - (width / 2);
    const points = [state.node];

    // Un punto por grado: el borde del sector queda parejo
    for (let i = 0; i <= width; i++) {
        points.push(destination(state.node, start + i, Number(radiusKm)));
    }
    points.push(state.node);

    const sector = L.polygon(points, {
        color: "#6f42c1",
        weight: 2,
        opacity: 0.9,
        fillColor: "#6f42c1",
        fillOpacity: 0.12
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
