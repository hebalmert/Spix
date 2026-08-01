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
