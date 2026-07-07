using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf.IO;

namespace Spix.xFiles.SignatureHelper;

public class PdfSignatureService : IPdfSignatureService
{
    private const string SignatureFieldName = "Signature";

    public byte[] FillPdf(byte[] templateBytes, IEnumerable<PdfSignatureField> fields, IDictionary<string, string?> values)
    {
        using var templateStream = new MemoryStream(templateBytes);
        var document = PdfReader.Open(templateStream, PdfDocumentOpenMode.Modify);

        foreach (var field in fields.Where(x => !IsSignatureField(x)))
        {
            if (!values.TryGetValue(field.FieldName, out var value) || string.IsNullOrWhiteSpace(value))
                continue;

            if (field.PageNumber < 1 || field.PageNumber > document.PageCount)
                continue;

            var page = document.Pages[field.PageNumber - 1];
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", field.FontSize, XFontStyle.Regular);

            gfx.DrawString(value, font, XBrushes.Black, new XPoint(field.PositionX, field.PositionY));
        }

        using var outputStream = new MemoryStream();
        document.Save(outputStream);
        return outputStream.ToArray();
    }

    public byte[] AddSignature(byte[] pdfBytes, PdfSignatureField signatureField, string signatureBase64)
    {
        if (string.IsNullOrWhiteSpace(signatureBase64))
            return pdfBytes;

        using var inputStream = new MemoryStream(pdfBytes);
        var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

        if (signatureField.PageNumber < 1 || signatureField.PageNumber > document.PageCount)
            return pdfBytes;

        var cleanBase64 = signatureBase64
            .Replace("data:image/png;base64,", string.Empty)
            .Trim();

        var page = document.Pages[signatureField.PageNumber - 1];
        var gfx = XGraphics.FromPdfPage(page);

        using var signatureStream = new MemoryStream(Convert.FromBase64String(cleanBase64));
        var signatureImage = XImage.FromStream(() => signatureStream);

        gfx.DrawImage(
            signatureImage,
            signatureField.PositionX,
            signatureField.PositionY,
            signatureField.Width ?? 200,
            signatureField.Height ?? 60);

        using var outputStream = new MemoryStream();
        document.Save(outputStream);
        return outputStream.ToArray();
    }

    public byte[] FillAndSignPdf(byte[] templateBytes, IEnumerable<PdfSignatureField> fields, IDictionary<string, string?> values, string signatureBase64)
    {
        var fieldList = fields.ToList();
        var pdfBytes = FillPdf(templateBytes, fieldList, values);
        var signatureField = fieldList.FirstOrDefault(IsSignatureField);

        return signatureField == null ? pdfBytes : AddSignature(pdfBytes, signatureField, signatureBase64);
    }

    private static bool IsSignatureField(PdfSignatureField field) =>
        string.Equals(field.FieldName, SignatureFieldName, StringComparison.OrdinalIgnoreCase);
}
