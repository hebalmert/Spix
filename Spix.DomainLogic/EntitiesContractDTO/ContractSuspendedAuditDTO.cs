namespace Spix.DomainLogic.EntitiesContractDTO;

public class ContractSuspendedAuditDTO
{
    public Guid ContractSuspendedAuditId { get; set; }

    public Guid ContractId { get; set; }

    public Guid ClientId { get; set; }

    public long ControlContrato { get; set; }

    public string ClientDocument { get; set; } = string.Empty;

    public string ClientFullName { get; set; } = string.Empty;

    public DateTime DateModified { get; set; }

    public string UserByName { get; set; } = string.Empty;
}
