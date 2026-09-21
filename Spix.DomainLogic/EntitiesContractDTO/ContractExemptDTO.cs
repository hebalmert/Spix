namespace Spix.DomainLogic.EntitiesContractDTO;

//Una exoneracion del registro (tabla ContractExempt). Los datos son la foto del momento,
//por eso no se leen de las otras tablas.
public class ExemptRecordDTO
{
    public Guid ContractExemptId { get; set; }

    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string? ClientName { get; set; }

    public string? ClientDocument { get; set; }

    public string? ContractPhone { get; set; }

    public string? ContractAddress { get; set; }

    public string? CityName { get; set; }

    public string? ZoneName { get; set; }

    public string? PlanName { get; set; }

    public decimal PlanAmount { get; set; }

    public DateTime DateExempt { get; set; }

    public DateTime? DateEnded { get; set; }

    public string? Motivo { get; set; }

    public string? UserByName { get; set; }

    public string? UserByNameEnded { get; set; }
}

//Listado del modulo con sus totales: el resumen lo calcula el backend sobre TODO el
//resultado del filtro, no sobre la pagina que se esta viendo.
public class ExemptListDTO
{
    public List<ExemptRecordDTO> Records { get; set; } = new();

    public int OpenCount { get; set; }

    public decimal OpenAmount { get; set; }

    public int TotalCount { get; set; }

    public decimal TotalAmount { get; set; }
}
