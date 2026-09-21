namespace Spix.Domain.EntitiesSchedule;

//Lo que el CLIENTE ve de su solicitud, y nada mas. No lleva montos, ni el detalle de lo
//cobrado, ni las fotos, ni el id del tecnico: si el dato no esta en este DTO, no hay forma
//de que se escape al portal.
public class MyServiceRequestItemDto
{
    public Guid ServiceRequestId { get; set; }

    public long RequestNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    //Nulo mientras la oficina no la agenda
    public DateTime? ScheduledAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public ScheduleStatus ScheduleStatus { get; set; }

    public long ControlContrato { get; set; }

    public string? Address { get; set; }

    public string? ZoneName { get; set; }

    //Lo que el cliente pidio
    public string? ClientReason { get; set; }

    //Y lo que le respondieron
    public string? TechnicianName { get; set; }

    public string? TechnicianComment { get; set; }

    public string? Recommendation { get; set; }

    //Solo las del DESPUES: son la prueba del trabajo hecho. Las del antes son
    //evidencia interna y no se consultan para el portal.
    public List<MyServiceRequestPhotoDto> Photos { get; set; } = new();
}

//Una foto del despues, como la ve el cliente
public class MyServiceRequestPhotoDto
{
    public Guid ServiceRequestPhotoId { get; set; }

    public DateTime DateCreated { get; set; }

    public string? ImageFullPath { get; set; }
}
