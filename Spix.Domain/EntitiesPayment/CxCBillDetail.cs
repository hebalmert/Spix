using Spix.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

public class CxCBillDetail
{
    [Key]
    public Guid CxCBillDetailId { get; set; }

    public Guid CxCBillId { get; set; }

    public DateTime DatePayment { get; set; }

    [MaxLength(30)]
    public string? PaymentMode { get; set; }

    [MaxLength(30)]
    public string? DiscountRate { get; set; }

    [MaxLength(256)]
    public string? Detail { get; set; }

    public decimal Debt { get; set; }

    public decimal Payment { get; set; }

    public decimal Discount { get; set; }

    public decimal Balance { get; set; }

    public decimal TotalPayments => Payment + Discount;

    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public CxCBill? CxCBill { get; set; }
}
