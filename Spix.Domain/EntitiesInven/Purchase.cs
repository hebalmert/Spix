using Spix.Domain.Entities;
using Spix.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class Purchase
{
    [Key]
    public Guid PurchaseId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Fecha de Compra")]
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Compra#")]
    public int NroPurchase { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Proveedor")]
    public Guid SupplierId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Bodega")]
    public Guid ProductStorageId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Fecha de Compra")]
    public DateTime FacuraDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Factura #")]
    public string NroFactura { get; set; } = null!;

    [Display(Name = "Estado")]
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pendiente;

    //Propiedades Virtuales

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Subtotal")]
    public decimal SubTotalCompra => PurchaseDetails == null ? 0 : PurchaseDetails.Sum(x => x.SubTotal);

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Impuesto")]
    public decimal ImpuestoTotalCompra => PurchaseDetails == null ? 0 : PurchaseDetails.Sum(x => x.Impuesto);

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Total")]
    public decimal TotalCompra => PurchaseDetails == null ? 0 : PurchaseDetails.Sum(x => x.TotalGeneral);

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    // Relaciones

    public Supplier? Supplier { get; set; }
    public ProductStorage? ProductStorage { get; set; }
    public ICollection<PurchaseDetail>? PurchaseDetails { get; set; }
    public ICollection<Cargue>? Cargue { get; set; }
}