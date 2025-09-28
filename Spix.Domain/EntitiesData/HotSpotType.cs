using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesData;

public class HotSpotType
{
    [Key]
    public int HotSpotTypeId { get; set; }

    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Tipo HotSpot")]
    public string TypeName { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Active { get; set; }
}