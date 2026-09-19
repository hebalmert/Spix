namespace Spix.xFiles.SignatureHelper;

public interface IPdfSignatureService
{
    byte[] FillPdf(byte[] templateBytes, IEnumerable<PdfSignatureField> fields, IDictionary<string, string?> values);

    byte[] AddSignature(byte[] pdfBytes, PdfSignatureField signatureField, string signatureBase64);

    byte[] FillAndSignPdf(byte[] templateBytes, IEnumerable<PdfSignatureField> fields, IDictionary<string, string?> values, string signatureBase64);

    //Sello de evidencia al pie de la ultima pagina (fecha, metodo, correo, IP)
    byte[] AddEvidenceFooter(byte[] pdfBytes, string text);

    //Hoja de certificado de firma electronica que se anexa al final del documento
    byte[] AddCertificatePage(byte[] pdfBytes, PdfCertificateData data);

    //Cierra el documento: se puede leer e imprimir, pero no editar
    byte[] Protect(byte[] pdfBytes);

    //Devuelve 0 si el archivo no es un PDF valido
    int GetPageCount(byte[] pdfBytes);
}
