namespace Spix.Domain.EntitiesSchedule;

public class ServiceRequestDetailDto
{
    public Guid ServiceRequestDetailId { get; set; }
    public Guid ServiceRequestId { get; set; }
    public Guid ServiceCategoryId { get; set; }
    public string? ServiceCategoryName { get; set; }
    public Guid ServiceClientId { get; set; }
    public string? ServiceClientName { get; set; }
    public string? Detail { get; set; }
}
