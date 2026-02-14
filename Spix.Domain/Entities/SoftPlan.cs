using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.Entities;

public class SoftPlan
{
    [Key]
    public int SoftPlanId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Plan), ResourceType = typeof(Resource))]
    public string? Name { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Price), ResourceType = typeof(Resource))]
    public decimal Price { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Months), ResourceType = typeof(Resource))]
    public int Meses { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Max_Clients), ResourceType = typeof(Resource))]
    public int ClientsCount { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    //Releaciones
    public ICollection<Corporation>? Corporations { get; set; }
}