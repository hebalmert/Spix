window.spixMap = window.spixMap || {};

window.spixMap.items = window.spixMap.items || {};

window.spixMap.render = (mapId, options) => {
    const element = document.getElementById(mapId);
    if (!element || !window.L) {
        return;
    }

    if (window.spixMap.items[mapId]) {
        window.spixMap.items[mapId].remove();
        delete window.spixMap.items[mapId];
    }

    const map = L.map(mapId, { scrollWheelZoom: true });
    window.spixMap.items[mapId] = map;

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

    const first = [Number(options.latitude), Number(options.longitude)];
    const points = [first];

    L.marker(first)
        .addTo(map)
        .bindPopup(options.firstLabel || "Cliente");

    if (options.secondLatitude !== null && options.secondLongitude !== null) {
        const second = [Number(options.secondLatitude), Number(options.secondLongitude)];
        points.push(second);

        L.marker(second)
            .addTo(map)
            .bindPopup(options.secondLabel || "Nodo");

        const line = L.polyline(points, { color: "#2563eb", weight: 4, opacity: 0.9 }).addTo(map);
        if (options.distanceText) {
            line.bindTooltip(options.distanceText, {
                permanent: true,
                direction: "center",
                className: "spix-map-distance"
            }).openTooltip();
        }

        map.fitBounds(line.getBounds(), { padding: [50, 50], maxZoom: 17 });
        return;
    }

    map.setView(first, 16);
};
