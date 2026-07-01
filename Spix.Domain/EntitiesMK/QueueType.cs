using Spix.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesMK;

public class QueueType
{

    [Key]
    public Guid QueueTypeId { get; set; }

    [Display(Name = "Queue Type")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string TypeName { get; set; } = null!;

    [Display(Name = "Down")]
    public bool Down { get; set; }

    [Display(Name = "Up")]
    public bool Up { get; set; }


    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Relaciones
    //...
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

}
