using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

public class ContractorPaymentDetail
{
    [Key]
    public Guid ContractorPaymentDetailId { get; set; }

    public Guid ContractorPaymentId { get; set; }

    public Guid ContractorAccountPayableId { get; set; }

    public decimal Payment { get; set; }

    public ContractorPayment? ContractorPayment { get; set; }

    public ContractorAccountPayable? ContractorAccountPayable { get; set; }
}
