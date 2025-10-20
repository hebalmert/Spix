namespace Spix.DomainLogic.ReportsDTO;

public class ReportDataDTO
{
    public int Id { get; set; }

    public Guid? GuidId { get; set; }

    public int TotalRegister { get; set; }

    public string? DateStart { get; set; }

    public string? DateEnd { get; set; }
}