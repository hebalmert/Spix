using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;
using QRCoder;

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

    //Sello visible al pie de la ultima pagina con la evidencia de la firma
    public byte[] AddEvidenceFooter(byte[] pdfBytes, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return pdfBytes;

        using var inputStream = new MemoryStream(pdfBytes);
        var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

        if (document.PageCount == 0)
            return pdfBytes;

        var page = document.Pages[document.PageCount - 1];
        using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
        var font = new XFont("Arial", 7, XFontStyle.Regular);

        gfx.DrawString(text, font, XBrushes.Gray, new XPoint(28, page.Height.Point - 18));

        using var outputStream = new MemoryStream();
        document.Save(outputStream);
        return outputStream.ToArray();
    }

    //Hoja final tipo "certificado de firma electronica": el documento se defiende solo,
    //sin tener que abrir la base de datos.
    public byte[] AddCertificatePage(byte[] pdfBytes, PdfCertificateData data)
    {
        using var inputStream = new MemoryStream(pdfBytes);
        var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

        var page = document.AddPage();
        page.Size = PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 15, XFontStyle.Bold);
        var subtitleFont = new XFont("Arial", 8.5, XFontStyle.Regular);
        var sectionFont = new XFont("Arial", 9.5, XFontStyle.Bold);
        var labelFont = new XFont("Arial", 7.5, XFontStyle.Regular);
        var valueFont = new XFont("Arial", 9, XFontStyle.Regular);
        var noteFont = new XFont("Arial", 7, XFontStyle.Italic);

        var headerBrush = new XSolidBrush(XColor.FromArgb(10, 26, 63));
        var sectionBrush = new XSolidBrush(XColor.FromArgb(20, 120, 255));
        var labelBrush = new XSolidBrush(XColor.FromArgb(108, 122, 148));
        var linePen = new XPen(XColor.FromArgb(220, 228, 242), 0.8);

        const double margin = 46;
        const double headerHeight = 74;
        var width = page.Width.Point;
        var valueX = margin + 150;
        var valueWidth = width - margin - valueX;

        //Banda superior
        gfx.DrawRectangle(headerBrush, 0, 0, width, headerHeight);
        gfx.DrawString(data.Title, titleFont, XBrushes.White, new XPoint(margin, 34));
        if (!string.IsNullOrWhiteSpace(data.Subtitle))
            gfx.DrawString(data.Subtitle, subtitleFont, XBrushes.White, new XPoint(margin, 54));

        //Recuadro de verificacion publica con codigo QR, arriba a la derecha
        var y = headerHeight + 34;
        var limit = page.Height.Point - 80;

        if (!string.IsNullOrWhiteSpace(data.VerificationCode) && !string.IsNullOrWhiteSpace(data.VerificationUrl))
        {
            DrawVerificationBox(gfx, data, width, margin, ref y, labelBrush, linePen);
        }

        foreach (var section in data.Sections)
        {
            if (y > limit)
                break;

            gfx.DrawString(section.Title.ToUpperInvariant(), sectionFont, sectionBrush, new XPoint(margin, y));
            gfx.DrawLine(linePen, margin, y + 6, width - margin, y + 6);
            y += 22;

            foreach (var item in section.Items)
            {
                if (y > limit)
                    break;

                gfx.DrawString(item.Label, labelFont, labelBrush, new XPoint(margin, y));
                y = DrawWrapped(gfx, item.Value, valueFont, XBrushes.Black, valueX, y, valueWidth);
                y += 15;
            }

            y += 10;
        }

        if (!string.IsNullOrWhiteSpace(data.Note))
        {
            var noteY = page.Height.Point - 54;
            gfx.DrawLine(linePen, margin, noteY - 14, width - margin, noteY - 14);
            DrawWrapped(gfx, data.Note!, noteFont, labelBrush, margin, noteY, width - (margin * 2));
        }

        using var outputStream = new MemoryStream();
        document.Save(outputStream);
        return outputStream.ToArray();
    }

    //Codigo QR + identificador publico: quien tenga el papel puede comprobar la firma en linea
    private static void DrawVerificationBox(XGraphics gfx, PdfCertificateData data, double width, double margin,
        ref double y, XBrush labelBrush, XPen linePen)
    {
        const double qrSize = 86;
        var boxHeight = qrSize + 18;
        var boxTop = y - 12;

        gfx.DrawRectangle(linePen, new XSolidBrush(XColor.FromArgb(248, 250, 255)), margin, boxTop, width - (margin * 2), boxHeight);

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(data.VerificationUrl!, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        using var qrStream = new MemoryStream(qrCode.GetGraphic(10));

        var qrImage = XImage.FromStream(() => qrStream);
        gfx.DrawImage(qrImage, width - margin - qrSize - 9, boxTop + 9, qrSize, qrSize);

        var codeFont = new XFont("Arial", 14, XFontStyle.Bold);
        var smallFont = new XFont("Arial", 7.5, XFontStyle.Regular);
        var linkFont = new XFont("Arial", 8, XFontStyle.Regular);

        var textX = margin + 14;
        gfx.DrawString("IDENTIFICADOR DE FIRMA", smallFont, labelBrush, new XPoint(textX, boxTop + 24));
        gfx.DrawString(data.VerificationCode!, codeFont, XBrushes.Black, new XPoint(textX, boxTop + 44));
        gfx.DrawString("Verifique este documento en:", smallFont, labelBrush, new XPoint(textX, boxTop + 66));
        gfx.DrawString(data.VerificationUrl!, linkFont, new XSolidBrush(XColor.FromArgb(20, 120, 255)), new XPoint(textX, boxTop + 80));

        y = boxTop + boxHeight + 30;
    }

    //Escribe el texto cortandolo por palabras; devuelve la Y de la ultima linea
    private static double DrawWrapped(XGraphics gfx, string text, XFont font, XBrush brush, double x, double y, double maxWidth)
    {
        var words = text.Split(' ');
        var line = string.Empty;

        foreach (var word in words)
        {
            var candidate = string.IsNullOrEmpty(line) ? word : $"{line} {word}";

            if (gfx.MeasureString(candidate, font).Width <= maxWidth)
            {
                line = candidate;
                continue;
            }

            //La palabra sola tampoco cabe (hash, user agent): se parte por caracteres
            if (string.IsNullOrEmpty(line))
            {
                foreach (var piece in SplitLong(gfx, word, font, maxWidth))
                {
                    gfx.DrawString(piece, font, brush, new XPoint(x, y));
                    y += 11;
                }

                y -= 11;
                continue;
            }

            gfx.DrawString(line, font, brush, new XPoint(x, y));
            y += 11;
            line = word;
        }

        if (!string.IsNullOrEmpty(line))
            gfx.DrawString(line, font, brush, new XPoint(x, y));

        return y;
    }

    private static IEnumerable<string> SplitLong(XGraphics gfx, string word, XFont font, double maxWidth)
    {
        var piece = string.Empty;

        foreach (var letter in word)
        {
            var candidate = piece + letter;

            if (gfx.MeasureString(candidate, font).Width > maxWidth && piece.Length > 0)
            {
                yield return piece;
                piece = letter.ToString();
                continue;
            }

            piece = candidate;
        }

        if (piece.Length > 0)
            yield return piece;
    }

    //El documento firmado queda cerrado: se lee y se imprime, pero no se puede editar.
    //La clave de propietario es aleatoria y no se guarda: nadie puede levantar las restricciones.
    public byte[] Protect(byte[] pdfBytes)
    {
        using var inputStream = new MemoryStream(pdfBytes);
        var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

        var security = document.SecuritySettings;
        security.OwnerPassword = Guid.NewGuid().ToString("N");
        security.PermitModifyDocument = false;
        security.PermitAssembleDocument = false;
        security.PermitFormsFill = false;
        security.PermitAnnotations = false;
        security.PermitPrint = true;
        security.PermitFullQualityPrint = true;
        security.PermitExtractContent = true;

        using var outputStream = new MemoryStream();
        document.Save(outputStream);
        return outputStream.ToArray();
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
