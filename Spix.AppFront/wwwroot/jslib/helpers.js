window.descargarDesdeUrl = function (url, fileName, mimeType) {
    fetch(url)
        .then(response => response.blob())
        .then(blob => {
            const blobUrl = URL.createObjectURL(new Blob([blob], { type: mimeType }));
            const link = document.createElement('a');
            link.href = blobUrl;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(blobUrl);
        });
};