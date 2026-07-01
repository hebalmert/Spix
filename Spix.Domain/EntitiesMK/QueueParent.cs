using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesMK;

public class QueueParent
{
    [Key]
    public Guid QueueParentId { get; set; }

    [Display(Name = "Parent Name")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string ParentName { get; set; } = null!;

    public Guid ServerId { get; set; }

    public Guid PlanId { get; set; }

    [Display(Name = "Down")]
    [MaxLength(15, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Down { get; set; } = null!;

    [Display(Name = "Up")]
    [MaxLength(15, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Up { get; set; } = null!;

    [Display(Name = "Mk Id")]
    [MaxLength(15, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string MkId { get; set; } = null!;

    //A que Corporacion Pertenece
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Server? Server { get; set; }

    public Plan? Plan { get; set; }
}
