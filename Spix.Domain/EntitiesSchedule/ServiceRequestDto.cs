namespace Spix.Domain.EntitiesSchedule;

public class ServiceRequestDto
{
    public Guid ServiceRequestId { get; set; }
    public long RequestNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public Guid ContractClientId { get; set; }
    public Guid TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public ScheduleStatus ScheduleStatus { get; set; } = ScheduleStatus.Pending;
    public string ClientReason { get; set; } = null!;
    public string? TechnicianComment { get; set; }
    public string? Recommendation { get; set; }
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
    public List<ServiceRequestDetailDto> Details { get; set; } = new();
}
