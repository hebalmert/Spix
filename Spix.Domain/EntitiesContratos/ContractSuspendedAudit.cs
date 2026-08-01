using Spix.Domain.Entities;
using Spix.Domain.EntitiesOper;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class ContractSuspendedAudit
{
    [Key]
    public Guid ContractSuspendedAuditId { get; set; }

    public Guid ContractId { get; set; }

    public Guid ClientId { get; set; }

    public DateTime DateModified { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(150)]
    public string UserByName { get; set; } = null!;

    public int CorporationId { get; set; }

    public ContractClient? ContractClient { get; set; }
    public Client? Client { get; set; }
    public Corporation? Corporation { get; set; }
}
