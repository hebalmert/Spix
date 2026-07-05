namespace Spix.Domain.EntitiesSchedule;

public class ServiceRequestDetailDto
{
    public Guid ServiceRequestDetailId { get; set; }
    public Guid ServiceRequestId { get; set; }
    public Guid ServiceCategoryId { get; set; }
    public string? ServiceCategoryName { get; set; }
    public Guid ServiceClientId { get; set; }
    public string? ServiceClientName { get; set; }
    public Guid? TaxId { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Price { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public Guid? SellDetailId { get; set; }
    public string? Detail { get; set; }
}
