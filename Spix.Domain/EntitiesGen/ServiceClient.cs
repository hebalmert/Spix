using Spix.Domain.Entities;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class ServiceClient
{
    [Key]
    public Guid ServiceClientId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Category), ResourceType = typeof(Resource))]
    public Guid ServiceCategoryId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Service), ResourceType = typeof(Resource))]
    public string ServiceName { get; set; } = null!;

    [MaxLength(250, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Description), ResourceType = typeof(Resource))]
    public string? Description { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Display(Name = nameof(Resource.Cost_Price), ResourceType = typeof(Resource))]
    public decimal Costo { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Tax), ResourceType = typeof(Resource))]
    public Guid TaxId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Display(Name = nameof(Resource.Price), ResourceType = typeof(Resource))]
    public decimal Price { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ServiceCategory? ServiceCategory { get; set; }

    public Tax? Tax { get; set; }

    //public ICollection<ContractClient>? ContractClients { get; set; }
}