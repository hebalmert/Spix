namespace Spix.DomainLogic.FileHandler;

public class FileBase64Result
{
    public string? Base64 { get; set; }
    public string MimeType { get; set; } = "application/octet-stream";
    public long SizeInBytes { get; set; }
}