namespace Spix.Domain.EntitiesSchedule;

public class ServiceRequestContractDto
{
    public Guid ContractClientId { get; set; }
    public long ControlContrato { get; set; }
    public string ClientFullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? CityName { get; set; }
    public string? ZoneName { get; set; }
    public string? ServerName { get; set; }
    public string? IpServer { get; set; }
    public string? IpCliente { get; set; }
    public string? MacCliente { get; set; }
    public string? PlanName { get; set; }
    public string? PlanSpeed { get; set; }
}
