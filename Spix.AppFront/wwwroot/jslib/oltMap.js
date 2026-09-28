// Mapa de OLT: JS propio de esta pantalla.
// Va aparte de nodeMap.js y de spixMap.js: no comparten nada, asi un cambio aqui no los toca.
// Solo se ejecuta cuando se abre /oltmap. Leaflet (window.L) ya viene cargado por index.html.
//
// No lleva mascara de cobertura: eso es el sector de un transmisor inalambrico y en fibra no
// significa nada.

window.spixOltMap = window.spixOltMap || {};

const oltMaps = {};

// El mapa en blanco con sus dos capas, comun a las dos vistas
function crear(mapId) {
    const element = document.getElementById(mapId);
    if (!element || !window.L) {
        return null;
    }

    window.spixOltMap.dispose(mapId);

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
// Las que no tienen coordenadas no se pueden pintar; el tablero las cuenta aparte.
window.spixOltMap.renderAll = function (mapId, olts, clientsLabel) {
    const state = crear(mapId);
    if (!state) {
        return;
    }

    (olts || []).forEach(olt => {
        if (olt.latitude === null || olt.longitude === null) {
            return;
        }

        const point = [Number(olt.latitude), Number(olt.longitude)];
        marcarOlt(state, point, "<b>" + olt.oltName + "</b><br/>" + (olt.ip || "") +
            "<br/>" + olt.clients + " " + clientsLabel);

        // El nombre queda escrito en el mapa: se busca la OLT de un vistazo, sin abrir popups
        L.marker(point, { opacity: 0 })
            .addTo(state.map)
            .bindTooltip(olt.oltName + " · " + olt.clients, {
                permanent: true,
                direction: "top",
                offset: [0, -10],
                className: "spix-map-oltname"
            });
    });

    encuadrar(state);
};

// Vista de UNA: la OLT y sus clientes como puntos, y las lineas segun la vista elegida
// (1 = solo puntos, 2 = lineas a la OLT, 3 = lineas con la distancia encima)
window.spixOltMap.render = function (mapId, data, view) {
    const state = crear(mapId);
    if (!state) {
        return;
    }

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
    if (!state) {
        return;
    }

    state.lines.clearLayers();

    // Sin coordenadas de la OLT no hay desde donde trazar
    if (!state.olt || Number(view) === 1) {
        return;
    }

    const withDistance = Number(view) === 3;
    state.clients.forEach(client => {
        const line = L.polyline([state.olt, client.point], { color: "#2563eb", weight: 2, opacity: 0.75 });
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

// Suelta el mapa (al cambiar de OLT o al salir de la pantalla)
window.spixOltMap.dispose = function (mapId) {
    const state = oltMaps[mapId];
    if (!state) {
        return;
    }

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
