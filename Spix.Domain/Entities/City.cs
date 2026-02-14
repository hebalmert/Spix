using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.Entities;

public class City
{
    [Key]
    public int CityId { get; set; }

    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    public int StateId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.City), ResourceType = typeof(Resource))]
    public string Name { get; set; } = null!;

    //Relaciones
    public State? State { get; set; }
}