using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Validations;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesNet;

//La OLT: el equipo de central por el que entran los clientes de FIBRA, igual que el Nodo
//es el AP por el que entran los de inalambrico. Por eso este modulo es el gemelo de Node.
//
//Aqui SOLO se guardan sus datos. Spix no se conecta a la OLT ni la configura: la IP, el
//usuario y la clave quedan registrados para que el tecnico sepa como entrarle.
public class Olt
{
    private decimal? _latitude;
    private decimal? _longitude;

    [Key]
    public Guid OltId { get; set; }

    [Display(Name = nameof(Resource.Olt), ResourceType = typeof(Resource))]
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public string OltName { get; set; } = null!;

    //Por donde se le entra. Sale del inventario de IP de RED, como el Nodo y el Servidor.
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.IpNetwork), ResourceType = typeof(Resource))]
    [ComboRequired]
    public Guid IpNetworkId { get; set; }

    [Display(Name = nameof(Resource.User), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public string Usuario { get; set; } = null!;

    [Display(Name = nameof(Resource.Password), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    //Obligatoria al crear (lo valida el servicio). Al editar puede llegar vacia: se conserva la guardada.
    public string Clave { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Mark), ResourceType = typeof(Resource))]
    [ComboRequired]
    public Guid MarkId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Model), ResourceType = typeof(Resource))]
    [ComboRequired]
    public Guid MarkModelId { get; set; }

    //Donde esta instalada
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Zone), ResourceType = typeof(Resource))]
    [ComboRequired]
    public Guid ZoneId { get; set; }

    [Range(-90, 90)]
    [Column(TypeName = "decimal(12,7)")]
    [Display(Name = nameof(Resource.Latitude), ResourceType = typeof(Resource))]
    public decimal? Latitude
    {
        get => _latitude;
        set => _latitude = value.HasValue ? Math.Round(value.Value, 7) : null;
    }

    [Range(-180, 180)]
    [Column(TypeName = "decimal(12,7)")]
    [Display(Name = nameof(Resource.Longitude), ResourceType = typeof(Resource))]
    public decimal? Longitude
    {
        get => _longitude;
        set => _longitude = value.HasValue ? Math.Round(value.Value, 7) : null;
    }

    //Cuantos puertos PON tiene el equipo
    [Range(1, 256, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.OltPorts), ResourceType = typeof(Resource))]
    public int PortCount { get; set; }

    //La velocidad de cada puerto, como la nombra el fabricante: 1G, 2.5G, 10G...
    [MaxLength(15, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.OltPortSpeed), ResourceType = typeof(Resource))]
    public string? PortSpeed { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    //Solo para encadenar los combos de la pantalla: la zona ya trae estado y ciudad
    [NotMapped]
    public int StateId { get; set; }

    [NotMapped]
    public int CityId { get; set; }

    //A que Corporacion Pertenece
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public IpNetwork? IpNetwork { get; set; }
    public Mark? Mark { get; set; }
    public MarkModel? MarkModel { get; set; }
    public Zone? Zone { get; set; }

    public ICollection<ContractOlt>? ContractOlts { get; set; }
}
