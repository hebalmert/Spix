using Spix.Domain.EntitiesSoft;

namespace Spix.AppInfra.FileHelper;

public interface IConsentPdfService
{
    byte[] AddSignatureToConsentPdf(byte[] pdfBytes, string signatureBase64, string language);

    byte[] GenerateConsentPdf(byte[] templateBytes, Patient data, string language);
}