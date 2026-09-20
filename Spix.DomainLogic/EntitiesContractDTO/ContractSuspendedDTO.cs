using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesContractDTO;

public class ContractSuspendedDTO
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string ClientDocument { get; set; } = null!;

    public string ClientFullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? CityName { get; set; }

    public string? ZoneName { get; set; }

    public string? PlanName { get; set; }
}

//Una suspension del registro (tabla ContractSuspended). Los datos son la foto del momento,
//por eso no se leen de las otras tablas.
public class SuspendedRecordDTO
{
    public Guid ContractSuspendedId { get; set; }

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

    public DateTime DateSuspended { get; set; }

    public DateTime? DateReactivated { get; set; }

    public SuspendedOrigin Origin { get; set; }

    public string? Motivo { get; set; }

    public string? UserByName { get; set; }

    public string? UserByNameReactivated { get; set; }
}

//Listado del modulo con sus totales: el resumen lo calcula el backend sobre TODO el
//resultado del filtro, no sobre la pagina que se esta viendo.
public class SuspendedListDTO
{
    public List<SuspendedRecordDTO> Records { get; set; } = new();

    public int OpenCount { get; set; }

    public decimal OpenAmount { get; set; }

    public int TotalCount { get; set; }

    public decimal TotalAmount { get; set; }
}

//Contrato activo que se puede suspender (para el autocompletar del Create)
public class ActiveContractDTO
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string? ClientName { get; set; }

    public string? ClientDocument { get; set; }

    public string? Address { get; set; }

    public string? PlanName { get; set; }

    public decimal PlanAmount { get; set; }
}
