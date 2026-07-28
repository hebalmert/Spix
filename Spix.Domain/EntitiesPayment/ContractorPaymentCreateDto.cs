using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

public class ContractorPaymentCreateDto
{
    public Guid ContractorId { get; set; }

    [Required]
    [MaxLength(30)]
    public string PaymentMode { get; set; } = "Cash";

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(250)]
    public string? Detail { get; set; }

    [MinLength(1)]
    public List<Guid> ContractorAccountPayableIds { get; set; } = new();
}
