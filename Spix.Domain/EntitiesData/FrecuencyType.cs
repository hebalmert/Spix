using Spix.Domain.EntitiesNet;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesData;

public class FrecuencyType
{
    [Key]
    public int FrecuencyTypeId { get; set; }

    [MaxLength(10, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Tipo Frecuencia")]
    public string TypeName { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Propiedad Virtual de consulta
    [Display(Name = "Frecuencias")]
    public int TotalFrecuencia => Frecuencies == null ? 0 : Frecuencies.Count;

    //Relaciones
    public ICollection<Frecuency>? Frecuencies { get; set; }

    public ICollection<Node>? Nodes { get; set; }
}