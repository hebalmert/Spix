using System.ComponentModel.DataAnnotations;
using Spix.Domain.Resources;

namespace Spix.Domain.Entities;

public class Country
{
    [Key]
    public int CountryId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Country")]
    public string Name { get; set; } = null!;

    [MaxLength(10, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Code Phone")]
    public string? CodPhone { get; set; }

    [Display(Name = "Total States")]
    public int StatesNumber => States == null ? 0 : States.Count;

    //relaciones
    public ICollection<State>? States { get; set; }

    public ICollection<Corporation>? Corporations { get; set; }
}