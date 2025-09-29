using Spix.Domain.Entities;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class ServiceCategory
{
    [Key]
    public Guid ServiceCategoryId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Categoria")]
    public string Name { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Propiedad Virtual de Consulta
    [Display(Name = "Servicios")]
    public int ServicesNumer => ServiceClients == null ? 0 : ServiceClients.Count;

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<ServiceClient>? ServiceClients { get; set; }

    //public ICollection<ContractClient>? ContractClients { get; set; }
}