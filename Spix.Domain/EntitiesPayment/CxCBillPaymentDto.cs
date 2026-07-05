using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

public class CxCBillPaymentDto
{
    public Guid CxCBillId { get; set; }

    [MaxLength(30)]
    public string PaymentMode { get; set; } = "Cash";

    public int DiscountPercent { get; set; }

    [MaxLength(256)]
    public string? Detail { get; set; }
}
