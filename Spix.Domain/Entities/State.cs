using System.ComponentModel.DataAnnotations;
using Spix.Domain.Resources;

namespace Spix.Domain.Entities;

public class State
{
    [Key]
    public int StateId { get; set; }

    public int CountryId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "State")]
    public string Name { get; set; } = null!;

    [Display(Name = "Total Cities")]
    public int CitiesNumber => Cities == null ? 0 : Cities.Count;

    //Relaciones
    public Country? Country { get; set; }

    public ICollection<City>? Cities { get; set; }
}