using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class TransferDetails
{
    [Key]
    public Guid TransferDetailsId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Tranferencia")]
    public Guid TransferId { get; set; }

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
    [Display(Name = "Cantidad")]
    public decimal Quantity { get; set; }

    //A que Corporacion Pertenece
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ProductCategory? ProductCategory { get; set; }

    public Product? Product { get; set; }

    public Transfer? Transfer { get; set; }
}