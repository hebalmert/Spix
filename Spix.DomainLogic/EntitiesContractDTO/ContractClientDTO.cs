using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesContractDTO;

public class ContractClientDTO
{
    public Guid ContractClientId { get; set; }

    public DateTime DateCreado { get; set; }

    public long ControlContrato { get; set; }

    public Guid ContractorId { get; set; }

    public Guid ClientId { get; set; }

    public string PhoneNumber { get; set; } = null!;
    public string? PhoneNumber2 { get; set; }
    public string Address { get; set; } = null!;

    public Guid ZoneId { get; set; }

    public ContractState ContractState { get; set; }

    public bool EquipoEmpres { get; set; }

    public bool EnvoiceClient { get; set; }
}
