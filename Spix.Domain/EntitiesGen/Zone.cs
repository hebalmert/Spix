using Spix.Core.EntitiesContratos;
using Spix.Core.EntitiesNet;
using Spix.Domain.Entities;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class Zone
{
    [Key]
    public Guid ZoneId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Depart/Estado")]
    public int StateId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Ciudad")]
    public int CityId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Zona")]
    public string ZoneName { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public State? state { get; set; }

    public City? city { get; set; }

    public ICollection<Node>? Nodes { get; set; }

    public ICollection<Server>? Servers { get; set; }

    public ICollection<ContractClient>? ContractClients { get; set; }
}