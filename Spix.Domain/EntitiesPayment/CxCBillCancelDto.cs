using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

public class CxCBillCancelDto
{
    public Guid CxCBillId { get; set; }

    [Required]
    [MaxLength(250)]
    public string DescriptionCancelled { get; set; } = null!;
}
