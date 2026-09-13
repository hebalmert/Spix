namespace Spix.xFiles.SignatureHelper;

public interface IPdfSignatureService
{
    byte[] FillPdf(byte[] templateBytes, IEnumerable<PdfSignatureField> fields, IDictionary<string, string?> values);

    byte[] AddSignature(byte[] pdfBytes, PdfSignatureField signatureField, string signatureBase64);

    byte[] FillAndSignPdf(byte[] templateBytes, IEnumerable<PdfSignatureField> fields, IDictionary<string, string?> values, string signatureBase64);

    //Devuelve 0 si el archivo no es un PDF valido
    int GetPageCount(byte[] pdfBytes);
}
