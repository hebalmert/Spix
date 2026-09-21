namespace Spix.Domain.EntitiesSchedule;

public class ServiceRequestDto
{
    public Guid ServiceRequestId { get; set; }
    public long RequestNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? UsuarioOwnerCompleted { get; set; }
    public Guid? UserIdCompleted { get; set; }
    public Guid ContractClientId { get; set; }
    public Guid? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public ScheduleStatus ScheduleStatus { get; set; } = ScheduleStatus.Pending;
    public ServiceRequestOrigin Origin { get; set; } = ServiceRequestOrigin.Office;
    public string? ContactPhone { get; set; }
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
    public string? NodeName { get; set; }
    public string? NodeIp { get; set; }
    public bool Billed { get; set; }
    public Guid? SellId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalTax { get; set; }
    public decimal Total { get; set; }
    public Guid? ServiceRequestPicId { get; set; }

    //Para el cierre guiado: si ya hay al menos una foto de cada lado
    public bool HasPhotoBefore { get; set; }

    public bool HasPhotoAfter { get; set; }
    public List<ServiceRequestDetailDto> Details { get; set; } = new();
    public List<ServiceRequestPhotoDto> Photos { get; set; } = new();
}

//Resumen del tablero: lo calcula el backend sobre TODAS las solicitudes que el usuario
//puede ver, no sobre la pagina que se esta viendo.
public class ServiceRequestSummaryDto
{
    public int Requested { get; set; }

    public int Today { get; set; }

    public int Pending { get; set; }

    public int InProgress { get; set; }

    public int CompletedMonth { get; set; }
}

//Una foto de la visita, como viaja a la pantalla
public class ServiceRequestPhotoDto
{
    public Guid ServiceRequestPhotoId { get; set; }

    public Guid ServiceRequestId { get; set; }

    public ServicePhotoType PhotoType { get; set; }

    public string? ImageFullPath { get; set; }

    //Lo que manda el navegador al subirla
    public string? ImgBase64 { get; set; }

    public DateTime DateCreated { get; set; }

    public string? UserByName { get; set; }
}
