window.cameraInterop2 = {
    startCamera: async () => {
        const video = document.getElementById("camera");
        video.style.display = "block";

        let constraints = {
            video: {
                facingMode: "environment",
                width: { ideal: 1920 },
                height: { ideal: 1080 }
            }
        };

        try {
            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            video.srcObject = stream;

            const track = stream.getVideoTracks()[0];
            const settings = track.getSettings();
            if (settings.deviceId) {
                localStorage.setItem("preferredCamera", settings.deviceId);
            }
        } catch (err) {
            const savedId = localStorage.getItem("preferredCamera");
            if (savedId) {
                const fallbackConstraints = { video: { deviceId: { exact: savedId } } };
                const fallbackStream = await navigator.mediaDevices.getUserMedia(fallbackConstraints);
                video.srcObject = fallbackStream;
                video.style.display = "block";
            } else {
                console.warn("No se pudo acceder a la cámara. Mostrando selector de archivo.");
                document.getElementById("fileInput").click();
            }
        }
    },

    stopCamera: function () {
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }

        const video = document.getElementById('camera');
        if (video) {
            video.srcObject = null;
        }
    },

    takePhoto: () => {
        const video = document.getElementById("camera");

        // Resolución máxima permitida
        const MAX_WIDTH = 1280;

        // Mantener proporción
        const ratio = video.videoHeight / video.videoWidth;

        let targetWidth = video.videoWidth;
        let targetHeight = video.videoHeight;

        // Si la imagen es muy grande, reducirla
        if (targetWidth > MAX_WIDTH) {
            targetWidth = MAX_WIDTH;
            targetHeight = MAX_WIDTH * ratio;
        }

        const canvas = document.createElement("canvas");
        canvas.width = targetWidth;
        canvas.height = targetHeight;

        const ctx = canvas.getContext("2d");
        ctx.drawImage(video, 0, 0, targetWidth, targetHeight);

        // JPEG calidad 0.8 (perfecto para OCR)
        return canvas.toDataURL("image/jpeg", 0.80);

    },

    loadFromFile: (dotNetRef) => {
        const input = document.getElementById("fileInput");
        input.onchange = () => {
            const file = input.files[0];
            if (!file) return;

            const reader = new FileReader();
            reader.onload = () => {
                dotNetRef.invokeMethodAsync("OnImageLoaded", reader.result);
            };
            reader.readAsDataURL(file);
        };
        input.click();
    }
};