using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class Cargue
{
    [Key]
    public Guid CargueId { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = false)]
    [Display(Name = "Fecha")]
    public DateTime DateCargue { get; set; }

    [Display(Name = "# Cargue")]
    public string? ControlCargue { get; set; }

    //Aca relacionamos con la compra al que pertenece el equipo
    [Display(Name = "Factura")]
    [MaxLength(20, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid PurchaseId { get; set; }

    [Display(Name = "Factura")]
    [MaxLength(20, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid PurchaseDetailId { get; set; }

    //Cantidad de seriales que responden a la cantidad de Itemn en PurchaseDetila
    [Column(TypeName = "decimal(18,2)")]
    public decimal CantToUp { get; set; }

    [Display(Name = "Producto")]
    public Guid ProductId { get; set; }

    [Display(Name = "Estado")]
    public CargueType Status { get; set; } = CargueType.Pendiente;

    [Display(Name = "Seriales")]
    public int TotalSeriales => CargueDetails == null ? 0 : CargueDetails.Count();

    //A que Corporacion Pertenece
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Purchase? Purchase { get; set; }

    public PurchaseDetail? PurchaseDetail { get; set; }

    public Product? Product { get; set; }

    public ICollection<CargueDetail>? CargueDetails { get; set; }
}