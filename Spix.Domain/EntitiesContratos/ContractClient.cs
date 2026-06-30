using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Spix.Domain.EntitiesContratos;

public class ContractClient
{
    [Key]
    public Guid ContractClientId { get; set; }


    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = false)]
    [Display(Name = nameof(Resource.Date), ResourceType = typeof(Resource))]
    public DateTime DateCreado { get; set; }

    [Display(Name = nameof(Resource.ContractNumber), ResourceType = typeof(Resource))]
    public long ControlContrato { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contractor), ResourceType = typeof(Resource))]
    public Guid ContractorId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Client), ResourceType = typeof(Resource))]
    public Guid ClientId { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(25, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string PhoneNumber { get; set; } = null!;

    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string? PhoneNumber2 { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(256, ErrorMessage = "El campo no puede ser mayor a {0} de largo")]
    [DataType(DataType.MultilineText)]
    [Display(Name = nameof(Resource.Address), ResourceType = typeof(Resource))]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = nameof(Resource.Zone), ResourceType = typeof(Resource))]
    public Guid ZoneId { get; set; }

    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    public ContractState ContractState { get; set; }

    [Display(Name = nameof(Resource.CompanyAntenna), ResourceType = typeof(Resource))]
    public bool EquipoEmpres { get; set; }

    [Display(Name = nameof(Resource.Invoice_Client), ResourceType = typeof(Resource))]
    public bool EnvoiceClient { get; set; }

    //Propiedades no Mapeadas para el Control de Contratos
    [Display(Name = nameof(Resource.UseHotSpotControl), ResourceType = typeof(Resource))]
    [NotMapped]
    public virtual bool UsaHotSpotControl { get; set; }

    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    [NotMapped]
    public virtual int StateId { get; set; }

    [Display(Name = nameof(Resource.City), ResourceType = typeof(Resource))]
    [NotMapped]
    public virtual int CityId { get; set; }

    [Display(Name = nameof(Resource.Client), ResourceType = typeof(Resource))]
    [NotMapped]
    public virtual string? NombreCliente { get; set; }

    [NotMapped]
    public virtual bool TieneIDPic => ContractIDPic != null;
    //Fin Propiedades no Mapeadas para el Control de Contratos

    public int CorporationId { get; set; }


    //Cual fue el usuario que Creo el Registro
    [Display(Name = "Quien Creo el QcGeneral")]  //El usuario que creo el QC su nombre en base al logueo del usuario.
    public string? UsuarioOwner { get; set; }

    [Display(Name = "Quien Creo el QcGeneral")]  //El UserId del usuario que lo creo para ubicarle sus credenciales.
    public Guid? UserId { get; set; }
    //Fin quien creo el registro

    public int ControlIpCount => ContractIps == null ? 0 : ContractIps.Count;
    public int ControlMacCount => ContractMacs == null ? 0 : ContractMacs.Count;
    public int ControlServerCount => ContractServers == null ? 0 : ContractServers.Count;
    public int ControlPlanCount => ContractPlans == null ? 0 : ContractPlans.Count;
    public int ControlNodeCount => ContractNodes == null ? 0 : ContractNodes.Count;


    public Corporation? Corporation { get; set; }
    public Contractor? Contractor { get; set; } 
    public Client? Client { get; set; }
    public Zone? Zone { get; set; } 


    public ContractIDPic? ContractIDPic { get; set; }
    public ICollection<ContractIp>? ContractIps { get; set; } 
    public ICollection<ContractMac>? ContractMacs { get; set; }
    public ICollection<ContractServer>? ContractServers { get; set; } 
    public ICollection<ContractPlan>? ContractPlans { get; set; }
    public ICollection<ContractNode>? ContractNodes { get; set; } 
    public ICollection<ContractQue>? ContractQues { get; set; } 
    public ICollection<ContractBind>? ContractBinds { get; set; } 

}
