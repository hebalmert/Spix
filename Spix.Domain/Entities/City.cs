using System.ComponentModel.DataAnnotations;
using Spix.Domain.Resources;

namespace Spix.Domain.Entities;

public class City
{
    [Key]
    public int CityId { get; set; }

    [Display(Name = "State")]
    public int StateId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "City")]
    public string Name { get; set; } = null!;

    //Relaciones
    public State? State { get; set; }
}