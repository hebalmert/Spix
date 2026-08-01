using Spix.Domain.EntitiesOper;
using Spix.Domain.EntitiesPayment;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class RunSuspendedDetail
{
    [Key]
    public Guid RunSuspendedDetailId { get; set; }

    public Guid RunSuspendedId { get; set; }

    public Guid ContractClientId { get; set; }

    public Guid ClientId { get; set; }

    public Guid CxCBillId { get; set; }

    public DateTime DateUtc { get; set; }

    public decimal PlanAmount { get; set; }

    public RunSuspended? RunSuspended { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Client? Client { get; set; }

    public CxCBill? CxCBill { get; set; }
}
