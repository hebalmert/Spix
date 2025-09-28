using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf.IO;
using Spix.Domain.EntitiesSoft;

namespace Spix.AppInfra.FileHelper;

public class ConsentPdfService : IConsentPdfService
{
    public byte[] GenerateConsentPdf(byte[] templateBytes, Patient data, string language)
    {
        using var templateStream = new MemoryStream(templateBytes);
        var doc = PdfReader.Open(templateStream, PdfDocumentOpenMode.Modify);
        var page = doc.Pages[0];
        var gfx = XGraphics.FromPdfPage(page);
        var font = new XFont("Arial", 12, XFontStyle.Regular);

        var coords = GetCoordinates(language);

        gfx.DrawString(data.FullName, font, XBrushes.Black, new XPoint(coords["FullName"].x, coords["FullName"].y));
        gfx.DrawString(Convert.ToString(data.DOB.ToString("MM/dd/yyyy")), font, XBrushes.Black, new XPoint(coords["DOB"].x, coords["DOB"].y));
        gfx.DrawString(data.PhoneCell, font, XBrushes.Black, new XPoint(coords["Phone"].x, coords["Phone"].y));
        gfx.DrawString(data.Address, font, XBrushes.Black, new XPoint(coords["Address"].x, coords["Address"].y));
        gfx.DrawString(Convert.ToString(data.Weight), font, XBrushes.Black, new XPoint(coords["Weight"].x, coords["Weight"].y));

        // Página 2: fecha y firma
        if (doc.PageCount > 1)
        {
            var page2 = doc.Pages[1];
            var gfx2 = XGraphics.FromPdfPage(page2);

            gfx2.DrawString(Convert.ToString(DateTime.Now.ToString("MM/dd/yyyy")), font, XBrushes.Black, new XPoint(coords["Date"].x, coords["Date"].y));
        }

        using var outputStream = new MemoryStream();
        doc.Save(outputStream);
        return outputStream.ToArray();
    }

    public byte[] AddSignatureToConsentPdf(byte[] pdfBytes, string signatureBase64, string language)
    {
        using var inputStream = new MemoryStream(pdfBytes);
        var doc = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

        if (doc.PageCount < 2 || string.IsNullOrWhiteSpace(signatureBase64))
            return pdfBytes; // No hay segunda página o firma inválida

        var coords = GetCoordinates(language);
        var page2 = doc.Pages[1];
        var gfx = XGraphics.FromPdfPage(page2);

        using var sigStream = new MemoryStream(Convert.FromBase64String(signatureBase64));
        var sigImage = XImage.FromStream(() => sigStream);

        gfx.DrawImage(sigImage, coords["Signature"].x, coords["Signature"].y, 200, 60);

        using var outputStream = new MemoryStream();
        doc.Save(outputStream);
        return outputStream.ToArray();
    }

    private Dictionary<string, (double x, double y)> GetCoordinates(string lang)
    {
        return lang == "en"
            ? new Dictionary<string, (double x, double y)>
            {
                ["FullName"] = (171.39, 216.06),
                ["DOB"] = (139.87, 243.31),
                ["Phone"] = (352.29, 243.64),
                ["Address"] = (113.27, 270.90),
                ["Weight"] = (109.99, 296.84),
                ["Date"] = (100.47, 619.61),
                ["Signature"] = (160.22, 624.34)
            }
            : new Dictionary<string, (double x, double y)>
            {
                ["FullName"] = (200.28, 203.58),
                ["DOB"] = (159.89, 230.18),
                ["Phone"] = (382.83, 230.18),
                ["Address"] = (123.45, 257.10),
                ["Weight"] = (98.50, 285.01),
                ["Date"] = (105.72, 669.19),
                ["Signature"] = (169.74, 671.45)
            };
    }
}