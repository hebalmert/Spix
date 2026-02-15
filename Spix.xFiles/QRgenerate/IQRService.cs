namespace Spix.xFiles.QRgenerate;

public interface IQRService
{
    string GenerateQrBase64(string url);
}