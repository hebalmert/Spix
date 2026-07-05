using Spix.Domain.Entities;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

public class CxCBill
{
    [Key]
    public Guid CxCBillId { get; set; }

    public DateTime DateNote { get; set; }

    [MaxLength(25)]
    public string? CollectionNote { get; set; }

    public Guid ClientId { get; set; }

    public Guid ContractClientId { get; set; }

    [MaxLength(250)]
    public string Description { get; set; } = null!;

    public decimal Total { get; set; }

    public decimal Balance { get; set; }

    public decimal TotalPayment => CxCBillDetails == null ? 0 : CxCBillDetails.Sum(x => x.Payment);

    public decimal TotalDiscount => CxCBillDetails == null ? 0 : CxCBillDetails.Sum(x => x.Discount);

    public decimal TotalPaid => CxCBillDetails == null ? 0 : CxCBillDetails.Sum(x => x.TotalPayments);

    public Guid SellId { get; set; }

    public Guid? BillingNoteOneId { get; set; }

    public bool Paid { get; set; }

    public DateTime? DatePaid { get; set; }

    public bool Cancelled { get; set; }

    public DateTime? DateCancelled { get; set; }

    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public Client? Client { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Sell? Sell { get; set; }

    public BillingNoteOne? BillingNoteOne { get; set; }

    public ICollection<CxCBillDetail>? CxCBillDetails { get; set; }

    public ICollection<PrePayment>? PrePayments { get; set; }

    public ICollection<PreExonerated>? PreExonerateds { get; set; }
}
