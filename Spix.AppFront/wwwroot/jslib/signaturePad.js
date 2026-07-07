let spixSignatureCanvas;
let spixSignatureCtx;
let spixSignatureDrawing = false;

window.spixSignature = {
    init: function (canvasId) {
        spixSignatureCanvas = document.getElementById(canvasId);
        if (!spixSignatureCanvas) return;

        spixSignatureCtx = spixSignatureCanvas.getContext("2d");
        spixSignatureCtx.strokeStyle = "#000";
        spixSignatureCtx.lineWidth = 2;
        spixSignatureCtx.lineCap = "round";

        spixSignatureCanvas.onmousedown = function (e) {
            spixSignatureDrawing = true;
            spixSignatureCtx.beginPath();
            spixSignatureCtx.moveTo(e.offsetX, e.offsetY);
        };

        spixSignatureCanvas.onmousemove = function (e) {
            if (!spixSignatureDrawing) return;
            spixSignatureCtx.lineTo(e.offsetX, e.offsetY);
            spixSignatureCtx.stroke();
        };

        spixSignatureCanvas.onmouseup = function () {
            spixSignatureDrawing = false;
        };

        spixSignatureCanvas.onmouseleave = function () {
            spixSignatureDrawing = false;
        };

        spixSignatureCanvas.ontouchstart = function (e) {
            e.preventDefault();
            spixSignatureDrawing = true;
            const touch = e.touches[0];
            const rect = spixSignatureCanvas.getBoundingClientRect();
            spixSignatureCtx.beginPath();
            spixSignatureCtx.moveTo(touch.clientX - rect.left, touch.clientY - rect.top);
        };

        spixSignatureCanvas.ontouchmove = function (e) {
            e.preventDefault();
            if (!spixSignatureDrawing) return;
            const touch = e.touches[0];
            const rect = spixSignatureCanvas.getBoundingClientRect();
            spixSignatureCtx.lineTo(touch.clientX - rect.left, touch.clientY - rect.top);
            spixSignatureCtx.stroke();
        };

        spixSignatureCanvas.ontouchend = function (e) {
            e.preventDefault();
            spixSignatureDrawing = false;
        };
    },
    clear: function () {
        if (!spixSignatureCanvas || !spixSignatureCtx) return;
        spixSignatureCtx.clearRect(0, 0, spixSignatureCanvas.width, spixSignatureCanvas.height);
        spixSignatureDrawing = false;
    },
    getBase64: function () {
        if (!spixSignatureCanvas) return "";
        return spixSignatureCanvas.toDataURL("image/png").replace("data:image/png;base64,", "");
    }
};
