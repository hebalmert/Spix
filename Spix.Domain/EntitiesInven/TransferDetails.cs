using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class TransferDetails
{
    [Key]
    public Guid TransferDetailsId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Transfer), ResourceType = typeof(Resource))]
    public Guid TransferId { get; set; }

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
    [Display(Name = nameof(Resource.Quantity), ResourceType = typeof(Resource))]
    public decimal Quantity { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public Product? Product { get; set; }
    public Transfer? Transfer { get; set; }

}