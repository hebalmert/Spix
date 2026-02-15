namespace Spix.xFiles.ExcelHelper;

public interface IExcelParser
{
    List<PatientControlExcel> ParseRecipientsFromBase64(string base64);
}

public class PatientControlExcel
{
    public DateTime BoD { get; set; }

    public string? ScreenRand { get; set; }

    public string? CellPhone { get; set; }

    public string? LastName { get; set; }

    public string? FirtName { get; set; }

    public int? MRN { get; set; }

    public string? Status { get; set; }
}