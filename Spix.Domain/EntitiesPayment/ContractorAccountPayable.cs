using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

public class ContractorAccountPayable
{
    [Key]
    public Guid ContractorAccountPayableId { get; set; }

    public DateTime DateCreated { get; set; }

    public Guid ContractorId { get; set; }

    public Guid ContractClientId { get; set; }

    public Guid CxCBillId { get; set; }

    public Guid CxCBillDetailId { get; set; }

    public decimal Rate { get; set; }

    public decimal BaseAmount { get; set; }

    public decimal Total { get; set; }

    public decimal Balance { get; set; }

    public bool Paid { get; set; }

    public DateTime? DatePaid { get; set; }

    public int CorporationId { get; set; }

    [MaxLength(150)]
    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Contractor? Contractor { get; set; }

    public ContractClient? ContractClient { get; set; }

    public CxCBill? CxCBill { get; set; }

    public CxCBillDetail? CxCBillDetail { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<ContractorPaymentDetail>? ContractorPaymentDetails { get; set; }
}
