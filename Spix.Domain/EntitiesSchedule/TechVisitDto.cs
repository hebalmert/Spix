namespace Spix.Domain.EntitiesSchedule;

//Lo que ve el tecnico en el telefono. DTO propio y no el de la web: la app pide poco y
//necesita cosas que la web no (la coordenada del contrato para medir, y si ya puede cerrar).
public class TechVisitDto
{
    public Guid ServiceRequestId { get; set; }

    public long RequestNumber { get; set; }

    public int Status { get; set; }

    public string? StatusText { get; set; }

    public string? OriginName { get; set; }

    public DateTime? ScheduledAtUtc { get; set; }

    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string? Address { get; set; }

    //A que numero hay que llamar para ESTA visita
    public string? ContactPhone { get; set; }

    public string? ClientReason { get; set; }

    //Lo que el tecnico necesita en la calle
    public string? PlanName { get; set; }

    public string? PlanSpeed { get; set; }

    public string? ServerName { get; set; }

    public string? NodeName { get; set; }

    public string? NodeIp { get; set; }

    public string? IpCliente { get; set; }

    public string? MacCliente { get; set; }

    //La del contrato: con esta la app mide la distancia sin tener que preguntar al servidor
    public decimal? ContractLatitude { get; set; }

    public decimal? ContractLongitude { get; set; }

    //Lo que ya marco el tecnico en esta visita
    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime? CapturedAtUtc { get; set; }

    public int? DistanceMeters { get; set; }

    public string? TechnicianComment { get; set; }

    //Las tres condiciones del cierre, resueltas en el servidor: la app solo prende o
    //apaga el boton, no vuelve a decidir la regla.
    public bool HasService { get; set; }

    public bool HasAfterPhoto { get; set; }

    public bool CanClose { get; set; }
}
