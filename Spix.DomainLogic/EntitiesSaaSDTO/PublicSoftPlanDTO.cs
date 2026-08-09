namespace Spix.DomainLogic.EntitiesSaaSDTO;

public class PublicSoftPlanDTO
{
    public int SoftPlanId { get; set; }
    public string Name { get; set; } = null!;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int ContractLimit { get; set; }
    public string? PublicDescription { get; set; }
    public bool IsRecommended { get; set; }
}
