using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.Models;
using Spix.xLanguage.Resources;

namespace Spix.xFiles.FileHelper;

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
                await blob.DeleteAsync();
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> SaveImageAsync(byte[] content, string fileName, string containerName)
    {
        var client = new BlobContainerClient(_azureOption.AzureStorage, containerName);
        await client.CreateIfNotExistsAsync();
        await client.SetAccessPolicyAsync(PublicAccessType.None);

        var blob = client.GetBlobClient(fileName);

        using var ms = new MemoryStream(content);
        await blob.UploadAsync(ms, overwrite: true);

        return fileName;
    }

    public async Task<string> SaveFileAsync(byte[] content, string fileName, string containerName, string fileNameOriginal, string mimeTypeEnviado)
    {
        var ext = Path.GetExtension(fileNameOriginal).ToLowerInvariant();
        var validExts = new[] { ".doc", ".docx", ".pdf" };

        if (!validExts.Contains(ext))
            throw new InvalidOperationException(_localizer[nameof(Resource.Errors_InvalidFileExtension)]);

        var mimeEsperado = GetMimeTypeExtended(fileNameOriginal);

        if (mimeEsperado?.Trim().ToLowerInvariant() != mimeTypeEnviado?.Trim().ToLowerInvariant())
            throw new InvalidOperationException(_localizer[nameof(Resource.Errors_InvalidMimeType)]);

        var client = new BlobContainerClient(_azureOption.AzureStorage, containerName);
        await client.CreateIfNotExistsAsync();
        await client.SetAccessPolicyAsync(PublicAccessType.None);

        var blob = client.GetBlobClient(fileName);

        var headers = new BlobHttpHeaders
        {
            ContentType = mimeEsperado,
            ContentDisposition = $"attachment; filename=\"{fileNameOriginal}\""
        };

        using var ms = new MemoryStream(content);
        var options = new BlobUploadOptions
        {
            HttpHeaders = headers,
            Conditions = new BlobRequestConditions()
        };

        await blob.UploadAsync(ms, options);

        return fileName;
    }

    public async Task<string?> GetBlobSasUrlAsync(string fileName, string containerName, TimeSpan validFor, bool forceDownload = false)
    {
        var containerClient = new BlobContainerClient(_azureOption.AzureStorage, containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync() || !blobClient.CanGenerateSasUri)
            return null;

        if (!forceDownload)
        {
            var props = await blobClient.GetPropertiesAsync();
            if (!string.IsNullOrEmpty(props.Value.ContentDisposition))
            {
                var headers = new BlobHttpHeaders
                {
                    ContentType = props.Value.ContentType,
                    ContentDisposition = null
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

    public async Task<Base64Result?> GetFileBase64Async(string? fileName, string containerName)
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

        var visualizables = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".gif" };
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        string? base64 = null;

        if (visualizables.Contains(ext))
        {
            var download = await blob.DownloadContentAsync();
            var bytes = download.Value.Content.ToArray();
            base64 = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }

        return new Base64Result
        {
            Base64 = base64,
            MimeType = mime,
            SizeInBytes = size
        };
    }

    public static string GetMimeTypeExtended(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
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
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            ".7z" => "application/x-7z-compressed",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            _ => "application/octet-stream"
        };
    }

    public async Task<string> UploadImage(IFormFile imageFile, string ruta, string guid)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), ruta, guid);

        using var stream = new FileStream(path, FileMode.Create);
        await imageFile.CopyToAsync(stream);

        return guid;
    }

    public async Task<string> UploadImage(byte[] imageFile, string ruta, string guid)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), ruta, guid);

        using var ms = new MemoryStream(imageFile);
        using var stream = new FileStream(path, FileMode.Create);
        await ms.CopyToAsync(stream);

        return guid;
    }

    public bool DeleteImage(string ruta, string guid)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), ruta, guid);

        if (File.Exists(path))
        {
            File.Delete(path);
            return true;
        }

        return false;
    }
}
