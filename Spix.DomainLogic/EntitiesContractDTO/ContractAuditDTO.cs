using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesContractDTO;

//Un paso de la linea de tiempo del contrato
public class ContractAuditDTO
{
    public Guid ContractAuditId { get; set; }

    public DateTime DateEvent { get; set; }

    public ContractEventType EventType { get; set; }

    public string? Detail { get; set; }

    public string? UserByName { get; set; }

    public string? SourceIp { get; set; }

    public Guid? ReferenceId { get; set; }
}

//La bitacora completa de un contrato, con su encabezado para saber de cual es
public class ContractAuditListDTO
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string? ClientName { get; set; }

    public string? ClientDocument { get; set; }

    public string? ContractAddress { get; set; }

    public List<ContractAuditDTO> Events { get; set; } = new();
}
