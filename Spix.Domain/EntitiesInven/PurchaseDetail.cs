using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class PurchaseDetail
{
    [Key]
    public Guid PurchaseDetailId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Purchase), ResourceType = typeof(Resource))]
    public Guid PurchaseId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Category), ResourceType = typeof(Resource))]
    public Guid ProductCategoryId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Product), ResourceType = typeof(Resource))]
    public Guid ProductId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Product), ResourceType = typeof(Resource))]
    public string? NameProduct { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Tax), ResourceType = typeof(Resource))]
    public decimal RateTax { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Quantity), ResourceType = typeof(Resource))]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.UnitCost), ResourceType = typeof(Resource))]
    public decimal UnitCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Subtotal), ResourceType = typeof(Resource))]
    public decimal SubTotal => Quantity * UnitCost;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Tax), ResourceType = typeof(Resource))]
    public decimal Impuesto => RateTax == 0 ? 0 : (((RateTax / 100) + 1) * SubTotal) - SubTotal;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Total), ResourceType = typeof(Resource))]
    public decimal TotalGeneral => SubTotal + Impuesto;

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public Purchase? Purchase { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public Product? Product { get; set; }
    public ICollection<Cargue>? Cargue { get; set; }

}