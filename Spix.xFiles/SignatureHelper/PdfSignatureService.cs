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

        // PdfSharp solo admite un XGraphics vivo por pagina, asi que los campos se agrupan
        // y cada pagina se dibuja de una sola pasada.
        var pages = fields
            .Where(x => !IsSignatureField(x))
            .Where(x => x.PageNumber >= 1 && x.PageNumber <= document.PageCount)
            .GroupBy(x => x.PageNumber);

        foreach (var pageFields in pages)
        {
            var page = document.Pages[pageFields.Key - 1];
            using var gfx = XGraphics.FromPdfPage(page);

            foreach (var field in pageFields)
            {
                if (!values.TryGetValue(field.FieldName, out var value) || string.IsNullOrWhiteSpace(value))
                    continue;

                var font = new XFont("Arial", field.FontSize, XFontStyle.Regular);

                gfx.DrawString(value, font, XBrushes.Black, new XPoint(field.PositionX, field.PositionY));
            }
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

        // Append deja la firma como lo ULTIMO que se dibuja en la pagina: queda encima del
        // texto y de las lineas de la plantilla, que es como se ve una firma de verdad.
        using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

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

        // Se estampa la firma en cada campo de firma que tenga la plantilla
        foreach (var signatureField in fieldList.Where(IsSignatureField))
        {
            pdfBytes = AddSignature(pdfBytes, signatureField, signatureBase64);
        }

        return pdfBytes;
    }

    public int GetPageCount(byte[] pdfBytes)
    {
        try
        {
            using var stream = new MemoryStream(pdfBytes);
            using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            return document.PageCount;
        }
        catch
        {
            return 0;
        }
    }

    private static bool IsSignatureField(PdfSignatureField field) =>
        string.Equals(field.FieldName, SignatureFieldName, StringComparison.OrdinalIgnoreCase);
}
