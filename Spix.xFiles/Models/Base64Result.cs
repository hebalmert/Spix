namespace Spix.xFiles.Models;

public class Base64Result
{
    public string? Base64 { get; set; }
    public string MimeType { get; set; } = "application/octet-stream";
    public long SizeInBytes { get; set; }
}
