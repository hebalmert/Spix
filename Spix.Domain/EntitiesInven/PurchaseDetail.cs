using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class PurchaseDetail
{
    [Key]
    public Guid PurchaseDetailId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Compra")]
    public Guid PurchaseId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Categoria")]
    public Guid ProductCategoryId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Producto")]
    public Guid ProductId { get; set; }

    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Display(Name = "Producto")]
    public string? NameProduct { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Impuesto")]
    public decimal RateTax { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Cantidad")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Costo Unitario")]
    public decimal UnitCost { get; set; }

    //Propiedades Virtuales

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Subtotal")]
    public decimal SubTotal => Quantity * UnitCost;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Impuesto")]
    public decimal Impuesto => RateTax == 0 ? 0 : (((RateTax / 100) + 1) * SubTotal) - SubTotal;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Total")]
    public decimal TotalGeneral => SubTotal + Impuesto;

    //A que Corporacion Pertenece
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    // Relaciones
    public Purchase? Purchase { get; set; }

    public ProductCategory? ProductCategory { get; set; }

    public Product? Product { get; set; }

    public ICollection<Cargue>? Cargue { get; set; }
}