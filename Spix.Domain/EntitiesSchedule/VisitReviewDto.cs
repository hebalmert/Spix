namespace Spix.Domain.EntitiesSchedule;

//Una fila de la bandeja. Solo para pintar la tarjeta: por eso trae los dos pares de
//coordenadas juntos y el nombre del tecnico ya resuelto.
public class VisitReviewDto
{
    public Guid ServiceRequestId { get; set; }

    public Guid ContractClientId { get; set; }

    public long RequestNumber { get; set; }

    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string? Address { get; set; }

    public string? TechnicianName { get; set; }

    //Como entro la visita (oficina, cliente o instalacion), ya traducido
    public string? OriginName { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public string? TechnicianComment { get; set; }

    //La que esta registrada en el contrato
    public decimal? ContractLatitude { get; set; }

    public decimal? ContractLongitude { get; set; }

    //Donde estuvo el tecnico
    public decimal? VisitLatitude { get; set; }

    public decimal? VisitLongitude { get; set; }

    public int? DistanceMeters { get; set; }

    //Si estuvo dentro del margen que se acepta como el mismo sitio
    public bool SameSite { get; set; }

    public bool ClientAbsent { get; set; }

    //Que numero de intento es esta visita en la cadena del contrato
    public int Attempt { get; set; }

    //Si ya se le creo una visita nueva, para no volver a ofrecer reagendar
    public bool AlreadyRescheduled { get; set; }
}

// Lo que va en el contador del menu y en las pestañas
public class VisitReviewCountersDto
{
    public int Location { get; set; }

    public int Absent { get; set; }

    public int Total => Location + Absent;
}
