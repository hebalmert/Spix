using Spix.Domain.Entities;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesGen;

public class Product
{
    [Key]
    public Guid ProductId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Categoria")]
    public Guid ProductCategoryId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Nombre")]
    public string ProductName { get; set; } = null!;

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Column(TypeName = "decimal(18,2)")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Display(Name = "Costo")]
    public decimal Costo { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Impuesto")]
    public Guid TaxId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Column(TypeName = "decimal(18,2)")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Display(Name = "Precio Venta Sin Iva")]
    public decimal Price { get; set; }

    [Display(Name = "Seriales")]
    public bool WithSerials { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ProductCategory? ProductCategory { get; set; }

    public Tax? Tax { get; set; }

    //Propiedades Virtuales

    public decimal TotalInventario => ProductStocks == null ? 0 : ProductStocks.Sum(x => x.Stock);

    //Releaciones en dos vias

    public ICollection<ProductStock>? ProductStocks { get; set; }

    public ICollection<PurchaseDetail>? PurchaseDetails { get; set; }

    public ICollection<TransferDetails>? TransferDetails { get; set; }

    public ICollection<Cargue>? Cargue { get; set; }
}