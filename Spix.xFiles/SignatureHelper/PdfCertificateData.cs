namespace Spix.xFiles.SignatureHelper;

//Datos que se imprimen en la hoja de certificado que se anexa al final del PDF firmado.
//Se arma en la capa de servicio: xFiles solo sabe dibujar titulos, etiquetas y valores.
public class PdfCertificateData
{
    public string Title { get; set; } = null!;

    public string? Subtitle { get; set; }

    public List<PdfCertificateSection> Sections { get; set; } = new();

    //Identificador publico de la firma y direccion donde cualquiera puede comprobarla.
    //Si vienen los dos, la hoja lleva codigo QR.
    public string? VerificationCode { get; set; }

    public string? VerificationUrl { get; set; }

    //Texto legal al pie de la hoja
    public string? Note { get; set; }
}

public class PdfCertificateSection
{
    public string Title { get; set; } = null!;

    public List<PdfCertificateItem> Items { get; set; } = new();
}

public class PdfCertificateItem
{
    public PdfCertificateItem(string label, string? value)
    {
        Label = label;
        Value = string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    public string Label { get; set; }

    public string Value { get; set; }
}
