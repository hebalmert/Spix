using Spix.Domain.Entities;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class PlanCategory
{
    [Key]
    public Guid PlanCategoryId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Categoria")]
    public string PlanCategoryName { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Propiedad Virtual de Consulta
    [Display(Name = "Planes")]
    public int PlanesNumer => Plans == null ? 0 : Plans.Count;

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<Plan>? Plans { get; set; }
}