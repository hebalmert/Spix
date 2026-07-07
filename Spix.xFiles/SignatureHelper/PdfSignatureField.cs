namespace Spix.xFiles.SignatureHelper;

public class PdfSignatureField
{
    public string FieldName { get; set; } = null!;

    public int PageNumber { get; set; }

    public double PositionX { get; set; }

    public double PositionY { get; set; }

    public double? Width { get; set; }

    public double? Height { get; set; }

    public int FontSize { get; set; } = 12;
}
