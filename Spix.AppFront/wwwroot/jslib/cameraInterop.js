window.cameraInterop = {
    startCamera: async () => {
        const video = document.getElementById("camera");
        video.style.display = "block";

        let constraints = { video: { facingMode: "environment" } };

        try {
            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            video.srcObject = stream;
            cameraInterop.stream = stream;

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
                cameraInterop.stream = fallbackStream;
                video.style.display = "block";
            } else {
                console.warn("No se pudo acceder a la cámara. Mostrando selector de archivo.");
                document.getElementById("fileInput").click();
            }
        }
    },

    //Suelta la camara: sin esto el navegador la sigue usando al salir de la pantalla
    stopCamera: function () {
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }

        const video = document.getElementById("camera");
        if (video) {
            video.srcObject = null;
        }
    },

    takePhoto: () => {
        const video = document.getElementById("camera");
        const canvas = document.createElement("canvas");
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        const ctx = canvas.getContext("2d");
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

        return canvas.toDataURL("image/jpeg", 0.95);
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