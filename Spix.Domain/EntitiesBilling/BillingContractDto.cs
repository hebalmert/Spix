namespace Spix.Domain.EntitiesBilling;

public class BillingContractDto
{
    public Guid ContractClientId { get; set; }

    public Guid ClientId { get; set; }

    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? CityName { get; set; }

    public string? ZoneName { get; set; }

    public string? PlanName { get; set; }

    public decimal? PlanPrice { get; set; }
}
