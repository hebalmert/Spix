using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesGen;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesSchedule;

public class ServiceRequestDetail
{
    [Key]
    public Guid ServiceRequestDetailId { get; set; }

    [Required]
    public Guid ServiceRequestId { get; set; }

    [Required]
    public Guid ServiceCategoryId { get; set; }

    [Required]
    public Guid ServiceClientId { get; set; }

    public Guid? TaxId { get; set; }

    public decimal TaxRate { get; set; }

    public decimal Price { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total => Price + TaxAmount;

    public Guid? SellDetailId { get; set; }

    [MaxLength(500)]
    public string? Detail { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public ServiceCategory? ServiceCategory { get; set; }

    public ServiceClient? ServiceClient { get; set; }

    public Tax? Tax { get; set; }

    public SellDetail? SellDetail { get; set; }
}
