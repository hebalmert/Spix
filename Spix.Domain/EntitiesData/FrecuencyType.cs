using Spix.Domain.EntitiesNet;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesData;

public class FrecuencyType
{
    [Key]
    public int FrecuencyTypeId { get; set; }

    [MaxLength(10, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.FrequencyType), ResourceType = typeof(Resource))]
    public string TypeName { get; set; } = null!;

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [Display(Name = nameof(Resource.Frequencies), ResourceType = typeof(Resource))]
    public int TotalFrecuencia => Frecuencies == null ? 0 : Frecuencies.Count;

    public ICollection<Frecuency>? Frecuencies { get; set; }
    public ICollection<Node>? Nodes { get; set; }

}