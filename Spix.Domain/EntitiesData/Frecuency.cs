using Spix.Domain.EntitiesNet;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesData;

public class Frecuency
{
    [Key]
    public int FrecuencyId { get; set; }

    [Display(Name = nameof(Resource.FrequencyType), ResourceType = typeof(Resource))]
    public int FrecuencyTypeId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Frequency), ResourceType = typeof(Resource))]
    public int FrecuencyName { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    public FrecuencyType? FrecuencyType { get; set; }
    public ICollection<Node>? Nodes { get; set; }

}