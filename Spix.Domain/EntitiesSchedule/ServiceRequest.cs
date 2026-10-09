using Spix.Domain.Entities;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations.Schema;
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

    //===== Donde estuvo el tecnico =====
    //Evidencia de ESTA visita, aparte de la del contrato (ContractMaps): si el tecnico
    //captura mal, el dato maestro no se corrompe sin rastro.
    [Range(-90, 90)]
    [Column(TypeName = "decimal(12,7)")]
    [Display(Name = nameof(Resource.Latitude), ResourceType = typeof(Resource))]
    public decimal? Latitude { get; set; }

    [Range(-180, 180)]
    [Column(TypeName = "decimal(12,7)")]
    [Display(Name = nameof(Resource.Longitude), ResourceType = typeof(Resource))]
    public decimal? Longitude { get; set; }

    [Display(Name = nameof(Resource.Date), ResourceType = typeof(Resource))]
    public DateTime? CapturedAtUtc { get; set; }

    //A que distancia quedo de la ubicacion del contrato, en metros. Null cuando el
    //contrato no tenia ubicacion con que comparar.
    public int? DistanceMeters { get; set; }

    //La visita que fallo y origino esta. Con esto se arma la cadena de intentos:
    //visita 1 sin cliente, visita 2 sin cliente, visita 3 hecha.
    public Guid? ServiceRequestParentId { get; set; }

    //El tecnico fue y no habia nadie. NO es anulacion: la visita se hizo y el resultado
    //fue que el cliente no estaba. Importa para medir al tecnico y no perder el intento.
    public bool ClientAbsent { get; set; }

    //La oficina ya la atendio en la bandeja (aplico, descarto o reagendo). Sin esto la
    //bandeja nunca se vacia.
    public bool LocationReviewed { get; set; }

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
