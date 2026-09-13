// Editor visual de campos para Plantillas PDF.
// Dibuja las paginas con pdf.js y encima los recuadros de los campos. Blazor manda la lista de campos
// (setFields) y el editor avisa cada accion del usuario (PlaceField, MoveField, SelectField).
// Coordenadas en puntos PDF medidas desde la esquina superior izquierda, igual que PdfSharp al llenar el PDF.
(function () {
    const PDFJS_VERSION = "3.11.174";
    const SIGNATURE = 5;
    // Textos de muestra por tipo de campo (mismos valores de prueba del servidor, ContractDocumentFieldType)
    const SAMPLES = {
        1: "Cliente Prueba Spix",
        2: "CC 1.220.478.524",
        3: "305-555-0100",
        4: "09/12/2026",
        6: "Calle 10 # 20-30 Barrio Centro",
        7: "cliente@correo.com",
        8: "Cliente Prueba"
    };

    let state = null;
    let pdfJsLoading = null;

    // Estilos de las paginas y recuadros: se agregan una sola vez al abrir el editor (no viven en app.css).
    const STYLES = `
        .spix-pdf-host.is-placing .spix-pdf-layer { cursor: crosshair; }
        .spix-pdf-page { position: relative; margin: 0 auto 16px; background: #fff; box-shadow: 0 4px 18px rgba(10,26,63,.18); }
        .spix-pdf-page canvas { display: block; }
        .spix-pdf-layer { position: absolute; inset: 0; }
        .spix-pdf-page-number { position: absolute; top: 6px; right: 8px; padding: 1px 8px; color: #fff; font-size: 11px; border-radius: 8px; background: rgba(10,26,63,.7); pointer-events: none; }
        .spix-pdf-field { position: absolute; display: flex; align-items: center; color: #0a1a3f; font-family: Arial, Helvetica, sans-serif; line-height: 1; white-space: nowrap; border: 1.5px dashed #0d6efd; background: rgba(13,110,253,.12); cursor: move; touch-action: none; user-select: none; }
        .spix-pdf-field.is-signature { justify-content: center; border-color: #198754; background: rgba(25,135,84,.12); }
        .spix-pdf-field.is-selected { border-style: solid; box-shadow: 0 0 0 3px rgba(13,110,253,.35); }
        .spix-pdf-field-label { position: absolute; bottom: 100%; left: -1.5px; padding: 0 5px; color: #fff; font-size: 10px; border-radius: 4px 4px 0 0; background: #0d6efd; pointer-events: none; }
        .spix-pdf-field.is-signature .spix-pdf-field-label { background: #198754; }
        .spix-pdf-field-sample { overflow: hidden; pointer-events: none; }
        .spix-pdf-handle { position: absolute; right: -7px; bottom: -7px; width: 14px; height: 14px; border: 2px solid #fff; border-radius: 50%; background: #198754; cursor: nwse-resize; }
    `;

    function ensureStyles() {
        if (document.getElementById("spix-pdf-field-editor-styles")) return;

        const style = document.createElement("style");
        style.id = "spix-pdf-field-editor-styles";
        style.textContent = STYLES;
        document.head.appendChild(style);
    }

    // pdf.js se carga SOLO cuando se abre el editor, nunca en el arranque de la aplicacion.
    function ensurePdfJs() {
        if (window.pdfjsLib) return Promise.resolve();

        pdfJsLoading ??= new Promise((resolve, reject) => {
            const script = document.createElement("script");
            script.src = `https://cdn.jsdelivr.net/npm/pdfjs-dist@${PDFJS_VERSION}/build/pdf.min.js`;
            script.onload = resolve;
            script.onerror = () => {
                pdfJsLoading = null;
                reject(new Error("No fue posible cargar pdf.js"));
            };
            document.head.appendChild(script);
        });

        return pdfJsLoading;
    }

    // El worker viene del CDN; se envuelve en un blob porque el navegador no crea workers de otro dominio.
    function ensureWorker() {
        if (pdfjsLib.GlobalWorkerOptions.workerSrc) return;

        const url = `https://cdn.jsdelivr.net/npm/pdfjs-dist@${PDFJS_VERSION}/build/pdf.worker.min.js`;
        const blob = new Blob([`importScripts("${url}");`], { type: "application/javascript" });
        pdfjsLib.GlobalWorkerOptions.workerSrc = URL.createObjectURL(blob);
    }

    function toBytes(base64) {
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
        return bytes;
    }

    // Recuadro (en puntos PDF) que ocupa un campo.
    // El texto se escribe sobre su linea base, por eso su recuadro empieza "tamano de letra" mas arriba.
    function boxOf(field) {
        if (field.fieldType === SIGNATURE) {
            return { left: field.positionX, top: field.positionY, width: field.width || 200, height: field.height || 60 };
        }

        const size = field.fontSize || 12;
        const sample = SAMPLES[field.fieldType] || "Texto";
        return { left: field.positionX, top: field.positionY - size, width: sample.length * size * 0.55, height: size * 1.3 };
    }

    // Posicion que se guarda a partir de la esquina superior izquierda del recuadro.
    function positionFromBox(fieldType, fontSize, left, top) {
        const y = fieldType === SIGNATURE ? top : top + (fontSize || 12);
        return { x: Math.max(0, left), y: Math.max(0, y) };
    }

    function place(el, box, scale) {
        el.style.left = `${box.left * scale}px`;
        el.style.top = `${box.top * scale}px`;
        el.style.width = `${box.width * scale}px`;
        el.style.height = `${box.height * scale}px`;
    }

    async function load(hostId, base64, dotNetRef, readOnly) {
        dispose();

        const host = document.getElementById(hostId);
        if (!host) return 0;

        ensureStyles();
        await ensurePdfJs();
        ensureWorker();

        const token = {};
        state = { token, host, dotNetRef, readOnly, pdf: null, pages: [], fields: [], selectedKey: null, placingType: 0, drag: null };
        host.innerHTML = "";
        host.classList.remove("is-placing");

        const pdf = await pdfjsLib.getDocument({ data: toBytes(base64) }).promise;
        if (state?.token !== token) { pdf.destroy(); return 0; }
        state.pdf = pdf;

        const available = Math.max(320, host.clientWidth - 40);
        const ratio = window.devicePixelRatio || 1;

        for (let number = 1; number <= pdf.numPages; number++) {
            const page = await pdf.getPage(number);
            if (state?.token !== token) return 0;

            const scale = Math.min(available / page.getViewport({ scale: 1 }).width, 2);
            const viewport = page.getViewport({ scale });

            const wrapper = document.createElement("div");
            wrapper.className = "spix-pdf-page";
            wrapper.style.width = `${viewport.width}px`;
            wrapper.style.height = `${viewport.height}px`;

            const canvas = document.createElement("canvas");
            canvas.width = Math.floor(viewport.width * ratio);
            canvas.height = Math.floor(viewport.height * ratio);
            canvas.style.width = `${viewport.width}px`;
            canvas.style.height = `${viewport.height}px`;

            const layer = document.createElement("div");
            layer.className = "spix-pdf-layer";

            const badge = document.createElement("span");
            badge.className = "spix-pdf-page-number";
            badge.textContent = `Pagina ${number}`;

            wrapper.append(canvas, layer, badge);
            host.appendChild(wrapper);

            await page.render({
                canvasContext: canvas.getContext("2d"),
                viewport,
                transform: ratio !== 1 ? [ratio, 0, 0, ratio, 0, 0] : null
            }).promise;

            if (state?.token !== token) return 0;

            if (!readOnly) layer.addEventListener("pointerdown", (e) => placeField(e, number));

            state.pages.push({
                number,
                scale,
                layer,
                width: viewport.width / scale,
                height: viewport.height / scale
            });
        }

        if (!readOnly) renderFields();
        return pdf.numPages;
    }

    function setFields(fields, selectedKey, placingType) {
        if (!state || state.readOnly) return;

        state.fields = fields || [];
        state.selectedKey = selectedKey;
        state.placingType = placingType || 0;
        state.host.classList.toggle("is-placing", state.placingType > 0);

        renderFields();
    }

    function renderFields() {
        state.pages.forEach((page) => { page.layer.innerHTML = ""; });

        state.fields.forEach((field) => {
            const page = state.pages[field.pageNumber - 1];
            if (!page) return;

            const isSignature = field.fieldType === SIGNATURE;
            const el = document.createElement("div");
            el.className = "spix-pdf-field"
                + (isSignature ? " is-signature" : "")
                + (field.key === state.selectedKey ? " is-selected" : "");
            el.dataset.key = field.key;
            place(el, boxOf(field), page.scale);

            const label = document.createElement("span");
            label.className = "spix-pdf-field-label";
            label.textContent = field.label;
            el.appendChild(label);

            // Texto de muestra al tamano real para ver como va a quedar
            const sample = document.createElement("span");
            sample.className = "spix-pdf-field-sample";
            sample.textContent = isSignature ? "Firma" : (SAMPLES[field.fieldType] || "");
            sample.style.fontSize = `${(isSignature ? 14 : (field.fontSize || 12)) * page.scale}px`;
            el.appendChild(sample);

            if (isSignature) {
                const handle = document.createElement("span");
                handle.className = "spix-pdf-handle";
                el.appendChild(handle);
            }

            el.addEventListener("pointerdown", (e) => startDrag(e, field, el, page));
            page.layer.appendChild(el);
        });
    }

    // Clic sobre la pagina (no sobre un recuadro) con un tipo de campo elegido: se coloca ahi.
    function placeField(e, pageNumber) {
        if (!state || !state.placingType || e.target !== e.currentTarget) return;

        const page = state.pages[pageNumber - 1];
        const rect = page.layer.getBoundingClientRect();
        const left = (e.clientX - rect.left) / page.scale;
        const top = (e.clientY - rect.top) / page.scale;
        const position = positionFromBox(state.placingType, 12, left, top);

        state.dotNetRef.invokeMethodAsync("PlaceField", pageNumber, position.x, position.y);
    }

    function startDrag(e, field, el, page) {
        e.preventDefault();
        e.stopPropagation();

        const box = boxOf(field);
        state.drag = {
            field, el, page, box,
            current: box,
            resizing: e.target.classList.contains("spix-pdf-handle"),
            startX: e.clientX,
            startY: e.clientY,
            moved: false
        };

        el.setPointerCapture(e.pointerId);
        el.onpointermove = dragMove;
        el.onpointerup = dragEnd;
    }

    function dragMove(e) {
        const drag = state?.drag;
        if (!drag) return;

        const dx = (e.clientX - drag.startX) / drag.page.scale;
        const dy = (e.clientY - drag.startY) / drag.page.scale;
        if (Math.abs(dx) + Math.abs(dy) > 1) drag.moved = true;

        const box = drag.box;
        if (drag.resizing) {
            drag.current = {
                ...box,
                width: clamp(box.width + dx, 40, drag.page.width - box.left),
                height: clamp(box.height + dy, 20, drag.page.height - box.top)
            };
        } else {
            drag.current = {
                ...box,
                left: clamp(box.left + dx, 0, drag.page.width - box.width),
                top: clamp(box.top + dy, 0, drag.page.height - box.height)
            };
        }

        place(drag.el, drag.current, drag.page.scale);
    }

    function dragEnd() {
        const drag = state?.drag;
        if (!drag) return;

        state.drag = null;
        drag.el.onpointermove = null;
        drag.el.onpointerup = null;

        if (!drag.moved) {
            state.dotNetRef.invokeMethodAsync("SelectField", drag.field.key);
            return;
        }

        const isSignature = drag.field.fieldType === SIGNATURE;
        const box = drag.current;
        const position = positionFromBox(drag.field.fieldType, drag.field.fontSize, box.left, box.top);

        state.dotNetRef.invokeMethodAsync(
            "MoveField",
            drag.field.key,
            position.x,
            position.y,
            isSignature ? box.width : null,
            isSignature ? box.height : null);
    }

    function scrollToField(key) {
        if (!state) return;
        const el = state.host.querySelector(`[data-key="${key}"]`);
        if (el) el.scrollIntoView({ behavior: "smooth", block: "center" });
    }

    function clamp(value, min, max) {
        return Math.min(Math.max(value, min), Math.max(min, max));
    }

    function dispose() {
        if (!state) return;
        if (state.pdf) state.pdf.destroy();
        state = null;
    }

    window.spixPdfFieldEditor = { load, setFields, scrollToField, dispose };
})();
