using Spix.Domain.Entities;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class ServiceCategory
{
    [Key]
    public Guid ServiceCategoryId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Category), ResourceType = typeof(Resource))]
    public string Name { get; set; } = null!;

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    //Propiedad Virtual de Consulta
    [Display(Name = nameof(Resource.Service), ResourceType = typeof(Resource))]
    public int ServicesNumer => ServiceClients == null ? 0 : ServiceClients.Count;

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<ServiceClient>? ServiceClients { get; set; }

    //public ICollection<ContractClient>? ContractClients { get; set; }
}