using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class ProductStock
{
    public Guid ProductStockId { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [Display(Name = "Producto")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [Display(Name = "Bodega")]
    public Guid ProductStorageId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Precio")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Stock { get; set; }

    // Método para actualizar el stock
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

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Product? Product { get; set; }

    public ProductStorage? ProductStorage { get; set; }
}