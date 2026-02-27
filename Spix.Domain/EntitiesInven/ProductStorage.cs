using Spix.Domain.Entities;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesInven;

public class ProductStorage
{
    [Key]
    public Guid ProductStorageId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Storage), ResourceType = typeof(Resource))]
    public string StorageName { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    public int StateId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.City), ResourceType = typeof(Resource))]
    public int CityId { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public State? State { get; set; }
    public City? City { get; set; }

    public ICollection<ProductStock>? ProductStocks { get; set; }
    public ICollection<Purchase>? Purchases { get; set; }

}