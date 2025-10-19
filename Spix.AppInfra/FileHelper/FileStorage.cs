using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Spix.Domain.Resources;
using Spix.DomainLogic.FileHandler;
using Spix.DomainLogic.ResponcesSec;

namespace Spix.AppInfra.FileHelper;

public class FileStorage : IFileStorage
{
    private readonly AzureSetting _azureOption;
    private readonly IStringLocalizer _localizer;

    public FileStorage(IOptions<AzureSetting> azureOption, IStringLocalizer localizer)
    {
        _azureOption = azureOption.Value;
        _localizer = localizer;
    }

    public async Task<bool> RemoveFileAsync(string containerName, string fileName)
    {
        try
        {
            var client = new BlobContainerClient(_azureOption.AzureStorage, containerName);
            var blob = client.GetBlobClient(fileName);

            if (await blob.ExistsAsync())
            {
                await blob.DeleteAsync(); // más explícito que DeleteIfExistsAsync
                return true;
            }

            return false;
        }
        catch (RequestFailedException ex)
        {
            // Aquí puedes loguear el error si lo necesitas
            Console.WriteLine($"Error al intentar borrar el archivo: {ex.Message}");
            return false;
        }
    }

    public async Task<string> SaveImageAsync(byte[] content, string fileName, string containerName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var validExts = new[] { ".jpg", ".jpeg", ".png" };

        if (!validExts.Contains(ext))
            throw new InvalidOperationException(_localizer[nameof(Resource.File_OnlyImage)]);

        var client = new BlobContainerClient(_azureOption.AzureStorage, containerName);
        await client.CreateIfNotExistsAsync();
        //client.SetAccessPolicy(PublicAccessType.Blob);
        await client.SetAccessPolicyAsync(PublicAccessType.None);

        var blob = client.GetBlobClient(fileName);

        using (var ms = new MemoryStream(content))
        {
            await blob.UploadAsync(ms, overwrite: true);
        }
        //Es para obtener la url completa junto con el archivo
        //return blob.Uri.ToString();
        return fileName;
    }

    public async Task<string> SaveFileAsync(byte[] content, string fileName, string containerName, string fileNameOriginal, string mimeTypeEnviado)
    {
        var ext = Path.GetExtension(fileNameOriginal).ToLowerInvariant();
        var validExts = new[] { ".doc", ".docx", ".pdf" };

        if (!validExts.Contains(ext))
            throw new InvalidOperationException(_localizer[nameof(Resource.File_OnlyFiles)]);

        var mimeEsperado = GetMimeTypeExtended(fileNameOriginal);

        if (mimeEsperado?.Trim().ToLowerInvariant() != mimeTypeEnviado?.Trim().ToLowerInvariant())
            throw new InvalidOperationException($"{_localizer[nameof(Resource.file_CheckMime)]} {mimeEsperado}, current: {mimeTypeEnviado}");

        var client = new BlobContainerClient(_azureOption.AzureStorage, containerName);
        await client.CreateIfNotExistsAsync();
        await client.SetAccessPolicyAsync(PublicAccessType.None); //Privado

        var blob = client.GetBlobClient(fileName);

        var headers = new BlobHttpHeaders
        {
            ContentType = mimeEsperado,
            ContentDisposition = $"attachment; filename=\"{fileNameOriginal}\""
        };

        using (var ms = new MemoryStream(content))
        {
            var options = new BlobUploadOptions
            {
                HttpHeaders = headers,
                Conditions = new BlobRequestConditions() // 👈 sin condiciones, permite overwrite
            };

            await blob.UploadAsync(ms, options); // 👈 sobrescribe sin borrar
        }

        return fileName;
    }

    public async Task<string?> GetBlobSasUrlAsync(string fileName, string containerName, TimeSpan validFor, bool forceDownload = false)
    {
        var containerClient = new BlobContainerClient(_azureOption.AzureStorage, containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync() || !blobClient.CanGenerateSasUri)
            return null;

        // 🔍 Diagnóstico: si el blob tiene Content-Disposition embebido, lo limpiamos
        if (!forceDownload)
        {
            var props = await blobClient.GetPropertiesAsync();
            if (!string.IsNullOrEmpty(props.Value.ContentDisposition))
            {
                var headers = new BlobHttpHeaders
                {
                    ContentType = props.Value.ContentType, // conservamos MIME
                    ContentDisposition = null // eliminamos el attachment
                };
                await blobClient.SetHttpHeadersAsync(headers);
            }
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = fileName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(validFor)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        if (forceDownload)
        {
            sasBuilder.ContentDisposition = $"attachment; filename={fileName}";
            sasBuilder.ContentType = "application/pdf";
        }

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    //Para poder leer el Blobs que esta en Privado
    public async Task<FileBase64Result?> GetFileBase64Async(string? fileName, string containerName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var client = new BlobContainerClient(_azureOption.AzureStorage, containerName);
        var blob = client.GetBlobClient(fileName);

        if (!await blob.ExistsAsync())
            return null;

        var properties = await blob.GetPropertiesAsync();
        var size = properties.Value.ContentLength;
        var mime = GetMimeTypeExtended(fileName);

        // Tipos visualizables
        var visualizables = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".png" };
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        string? base64 = null;
        if (visualizables.Contains(ext))
        {
            var download = await blob.DownloadContentAsync();
            var bytes = download.Value.Content.ToArray();
            base64 = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }

        return new FileBase64Result
        {
            Base64 = base64, // null si no es visualizable
            MimeType = mime,
            SizeInBytes = size
        };
    }

    public static string GetMimeTypeExtended(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            // Imágenes
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",

            // Documentos
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".rtf" => "application/rtf",
            ".md" => "text/markdown",
            ".json" => "application/json",
            ".xml" => "application/xml",

            // Audio
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",

            // Video
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",

            // Comprimidos
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            ".7z" => "application/x-7z-compressed",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",

            // Web
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",

            // Fallback
            _ => "application/octet-stream"
        };
    }

    //Para Guardado de Imagenes en Disco
    //Solo pra alamcenamiento local o uso de IFormFile
    public async Task<string> UploadImage(IFormFile imageFile, string ruta, string guid)
    {
        var file = guid;
        var path = Path.Combine(
            Directory.GetCurrentDirectory(),
            ruta,
            file);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        return $"{file}";
    }

    public async Task<string> UploadImage(byte[] imageFile, string ruta, string guid)
    {
        var file = guid;
        var path = Path.Combine(
            Directory.GetCurrentDirectory(),
            ruta,
            file);

        var NIformFile = new MemoryStream(imageFile);
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await NIformFile.CopyToAsync(stream);
        }

        return $"{file}";
    }

    public bool DeleteImage(string ruta, string guid)
    {
        string path;
        path = Path.Combine(
            Directory.GetCurrentDirectory(),
            ruta,
            guid);

        File.Delete(path);

        return true;
    }
}