namespace Spix.DomainLogic.EntitiesDashboardDTO;

public class DashboardSummaryDto
{
    public int ActiveContracts { get; set; }

    public int SuspendedContracts { get; set; }

    public decimal MonthTotal { get; set; }

    public decimal MonthCollected { get; set; }

    public decimal MonthBalance { get; set; }
}
