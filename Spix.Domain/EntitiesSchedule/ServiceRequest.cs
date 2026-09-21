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

    //Nulos mientras la solicitud no se agenda (la pidio el cliente y falta revisarla)
    public DateTime? ScheduledAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public string? UsuarioOwnerCompleted { get; set; }

    public Guid? UserIdCompleted { get; set; }

    [Required]
    public Guid ContractClientId { get; set; }

    //Sin [Required]: la solicitud del cliente nace sin tecnico, la oficina lo asigna
    public Guid? TechnicianId { get; set; }

    public ScheduleStatus ScheduleStatus { get; set; } = ScheduleStatus.Pending;

    //Con que numero hay que llamar para esta visita: puede no ser el del contrato,
    //porque los clientes cambian de numero.
    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    //Si la levanto la oficina o el propio cliente desde su portal
    public ServiceRequestOrigin Origin { get; set; } = ServiceRequestOrigin.Office;

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

    //Por donde entra el cliente: el tecnico lo necesita para resetear el transmisor
    [MaxLength(100)]
    public string? NodeName { get; set; }

    [MaxLength(50)]
    public string? NodeIp { get; set; }

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

    //Las fotos de la visita: cada una es un registro
    public ICollection<ServiceRequestPhoto>? ServiceRequestPhotos { get; set; }

    public decimal SubTotal => ServiceRequestDetails == null ? 0 : ServiceRequestDetails.Sum(x => x.Price);

    public decimal TotalTax => ServiceRequestDetails == null ? 0 : ServiceRequestDetails.Sum(x => x.TaxAmount);

    public decimal Total => ServiceRequestDetails == null ? 0 : ServiceRequestDetails.Sum(x => x.Total);
}
