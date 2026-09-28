using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Spix.DomainLogic.Validations;

namespace Spix.Domain.EntitiesNet;

public class Server
{
    [Key]
    public Guid ServerId { get; set; }

    [Display(Name = nameof(Resource.Server), ResourceType = typeof(Resource))]
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public string ServerName { get; set; } = null!;

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

    //Las dos interfaces del equipo se eligen en el detalle del servidor, leyendolas del
    //propio MikroTik: al crear el servidor todavia no se sabe si se puede entrar.
    [Display(Name = nameof(Resource.WanName), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    public string? WanName { get; set; }

    [Display(Name = nameof(Resource.LanName), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    public string? LanName { get; set; }

    [Display(Name = nameof(Resource.ApiPort), ResourceType = typeof(Resource))]
    [Range(1, 65535, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int ApiPort { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Mark), ResourceType = typeof(Resource))]
    [ComboRequired]
    public Guid MarkId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Model), ResourceType = typeof(Resource))]
    [ComboRequired]
    public Guid MarkModelId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Zone), ResourceType = typeof(Resource))]
    [ComboRequired]
    public Guid ZoneId { get; set; }

    //Como trabaja ESTE equipo. Un servidor hace una cosa: PPPoE o HotSpot, no las dos.
    //Mezclarlas en la misma interfaz consume recursos y MikroTik lo desaconseja.
    [Display(Name = nameof(Resource.MikrotikControl), ResourceType = typeof(Resource))]
    public MikrotikControlType ControlMk { get; set; } = MikrotikControlType.Ninguno;

    //La IP del router del lado de los clientes: viaja al equipo como local-address del
    //perfil PPPoE. Sale del inventario de IP de RED (IpNetwork), no del de clientes:
    //es la puerta de enlace del equipo, no una IP que se le entregue a nadie.
    //Igual se marca Excluded y Assigned, para que no se ofrezca dos veces.
    [Display(Name = nameof(Resource.PppLocalIp), ResourceType = typeof(Resource))]
    public Guid? PppLocalIpNetId { get; set; }

    //Un solo perfil por equipo: la velocidad la maneja el Queue con la tasa de reuso,
    //asi que el rate-limit del perfil va vacio y no hace falta un perfil por plan.
    [Display(Name = nameof(Resource.PppProfileName), ResourceType = typeof(Resource))]
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    public string? PppProfileName { get; set; }

    [MaxLength(15, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    public string? PppProfileMkId { get; set; }

    [Display(Name = nameof(Resource.PppServiceName), ResourceType = typeof(Resource))]
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    public string? PppServiceName { get; set; }

    [MaxLength(15, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    public string? PppServerMkId { get; set; }

    //El nombre del equipo segun /system/identity, aprendido al probar la conexion.
    //
    //Sirve para una sola cosa: la IP de gestion de un router CAMBIA en la vida real
    //(renumeracion, otro proveedor), y cuando cambia sigue siendo el mismo equipo, asi que
    //los MikrotikId guardados siguen valiendo. Bloquear el cambio romperia esa operacion
    //legitima. Guardando la identidad, Spix puede conectarse a la IP nueva y comprobar que
    //del otro lado esta el MISMO equipo antes de aceptar el cambio.
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    public string? MkIdentity { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [NotMapped]
    public int StateId { get; set; }

    [NotMapped]
    public int CityId { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public IpNetwork? IpNetwork { get; set; }
    public IpNetwork? PppLocalIpNet { get; set; }
    public Mark? Mark { get; set; }
    public MarkModel? MarkModel { get; set; }
    public Zone? Zone { get; set; }

    public ICollection<ContractServer>? ContractServers { get; set; }
    public ICollection<ContractQue>? ContractQues { get; set; }
    public ICollection<ContractBind>? ContractBinds { get; set; }
    public ICollection<ContractPppoe>? ContractPppoes { get; set; }
    public ICollection<QueueParent>? QueueParents { get; set; }
}