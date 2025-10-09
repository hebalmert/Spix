using Spix.Domain.Entities;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class ProductCategory
{
    [Key]
    public Guid ProductCategoryId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Categoria")]
    public string Name { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Propiedad Virtual de Consulta
    [Display(Name = "Productos")]
    public int ProductsNumer => Products == null ? 0 : Products.Count;

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<Product>? Products { get; set; }

    public ICollection<PurchaseDetail>? PurchaseDetails { get; set; }

    public ICollection<TransferDetails>? TransferDetails { get; set; }
}