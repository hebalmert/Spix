using Microsoft.AspNetCore.Http;
using Spix.xFiles.Models;

namespace Spix.xFiles.FileHelper;

public interface IFileStorage
{
    //Manejo de Imagenes para AZURE Containers

    Task<bool> RemoveFileAsync(string containerName, string fileName);

    Task<string> SaveFileAsync(byte[] content, string fileName, string containerName, string fileNameOriginal, string mimeTypeEnviado);

    Task<string> SaveImageAsync(byte[] content, string fileName, string containerName);

    Task<Base64Result?> GetFileBase64Async(string? fileName, string containerName);

    Task<string?> GetBlobSasUrlAsync(string fileName, string containerName, TimeSpan validFor, bool forceDownload = false);

    //Para Guardado en Disco

    Task<string> UploadImage(IFormFile imageFile, string ruta, string guid);

    Task<string> UploadImage(byte[] imageFile, string ruta, string guid);

    bool DeleteImage(string ruta, string guid);
}