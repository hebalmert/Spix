using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class ProductStock
{
    [Key]
    public Guid ProductStockId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Product), ResourceType = typeof(Resource))]
    public Guid ProductId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Storage), ResourceType = typeof(Resource))]
    public Guid ProductStorageId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Price), ResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public decimal Stock { get; set; }

    public void AddStock(decimal quantity)
    {
        Stock += quantity;
    }

    public void ReduceStock(decimal quantity)
    {
        if (Stock >= quantity)
            Stock -= quantity;
        else
            throw new InvalidOperationException("No hay suficiente stock disponible.");
    }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public Product? Product { get; set; }
    public ProductStorage? ProductStorage { get; set; }

}