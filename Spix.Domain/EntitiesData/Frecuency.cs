using Spix.Core.EntitiesNet;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesData;

public class Frecuency
{
    [Key]
    public int FrecuencyId { get; set; }

    [Display(Name = "Tipo Frecuencia")]
    public int FrecuencyTypeId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Frecuencia")]
    public int FrecuencyName { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Relaciones
    public FrecuencyType? FrecuencyType { get; set; }

    public ICollection<Node>? Nodes { get; set; }
}