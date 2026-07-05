using Spix.Domain.Entities;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesSchedule;

public class ServiceRequest
{
    [Key]
    public Guid ServiceRequestId { get; set; }

    public long RequestNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ScheduledAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    [Required]
    public Guid ContractClientId { get; set; }

    [Required]
    public Guid TechnicianId { get; set; }

    public ScheduleStatus ScheduleStatus { get; set; } = ScheduleStatus.Pending;

    [Required]
    [MaxLength(500)]
    public string ClientReason { get; set; } = null!;

    [MaxLength(1000)]
    public string? TechnicianComment { get; set; }

    [MaxLength(1000)]
    public string? Recommendation { get; set; }

    public long ControlContrato { get; set; }

    [MaxLength(150)]
    public string ClientFullName { get; set; } = null!;

    [MaxLength(25)]
    public string? PhoneNumber { get; set; }

    [MaxLength(256)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? CityName { get; set; }

    [MaxLength(100)]
    public string? ZoneName { get; set; }

    [MaxLength(100)]
    public string? ServerName { get; set; }

    [MaxLength(100)]
    public string? IpServer { get; set; }

    [MaxLength(100)]
    public string? IpCliente { get; set; }

    [MaxLength(100)]
    public string? MacCliente { get; set; }

    [MaxLength(100)]
    public string? PlanName { get; set; }

    [MaxLength(100)]
    public string? PlanSpeed { get; set; }

    public bool Active { get; set; } = true;

    public bool Billed { get; set; }

    public Guid? SellId { get; set; }

    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Technician? Technician { get; set; }

    public ScheduleItem? ScheduleItem { get; set; }

    public ServiceRequestPic? ServiceRequestPic { get; set; }

    public Sell? Sell { get; set; }

    public ICollection<ServiceRequestDetail>? ServiceRequestDetails { get; set; }

    public decimal SubTotal => ServiceRequestDetails == null ? 0 : ServiceRequestDetails.Sum(x => x.Price);

    public decimal TotalTax => ServiceRequestDetails == null ? 0 : ServiceRequestDetails.Sum(x => x.TaxAmount);

    public decimal Total => ServiceRequestDetails == null ? 0 : ServiceRequestDetails.Sum(x => x.Total);
}
