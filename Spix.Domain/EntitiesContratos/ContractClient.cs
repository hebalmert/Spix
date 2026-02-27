using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesContratos;

public class ContractClient
{
    [Key]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = false)]
    [Display(Name = nameof(Resource.Date), ResourceType = typeof(Resource))]
    public DateTime DateCreado { get; set; }

    [Display(Name = nameof(Resource.ContractNumber), ResourceType = typeof(Resource))]
    public string? ControlContrato { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contractor), ResourceType = typeof(Resource))]
    public Guid ContractorId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Client), ResourceType = typeof(Resource))]
    public Guid ClientId { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(7, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = nameof(Resource.Country), ResourceType = typeof(Resource))]
    public string CodeCountry { get; set; } = null!;

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(3, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = nameof(Resource.Code), ResourceType = typeof(Resource))]
    public string CodeNumber { get; set; } = null!;

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(7, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(256, ErrorMessage = "El campo no puede ser mayor a {0} de largo")]
    [DataType(DataType.MultilineText)]
    [Display(Name = nameof(Resource.Address), ResourceType = typeof(Resource))]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = nameof(Resource.Zone), ResourceType = typeof(Resource))]
    public Guid ZoneId { get; set; }

    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    public StateType StateType { get; set; }

    [Display(Name = nameof(Resource.CompanyAntenna), ResourceType = typeof(Resource))]
    public bool EquipoEmpres { get; set; }

    [Display(Name = nameof(Resource.InvoiceClient), ResourceType = typeof(Resource))]
    public bool EnvoiceClient { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = nameof(Resource.ServiceCategory), ResourceType = typeof(Resource))]
    public Guid ServiceCategoryId { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = nameof(Resource.ClientService), ResourceType = typeof(Resource))]
    public Guid ServiceClientId { get; set; }

    [MaxLength(50, ErrorMessage = "El Maximo de caracteres es {0}")]
    [Display(Name = nameof(Resource.Service), ResourceType = typeof(Resource))]
    public string? ServiceName { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El Valor del Precio debe ser mayor que {1}")]
    [Display(Name = nameof(Resource.Tax), ResourceType = typeof(Resource))]
    public decimal? Impuesto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El Valor del Precio debe ser mayor que {1}")]
    [Display(Name = nameof(Resource.Price), ResourceType = typeof(Resource))]
    public decimal? Price { get; set; }

    [Display(Name = nameof(Resource.UseHotSpotControl), ResourceType = typeof(Resource))]
    [NotMapped]
    public bool UsaHotSpotControl { get; set; }

    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    [NotMapped]
    public int StateId { get; set; }

    [Display(Name = nameof(Resource.City), ResourceType = typeof(Resource))]
    [NotMapped]
    public int CityId { get; set; }

    [Display(Name = nameof(Resource.Client), ResourceType = typeof(Resource))]
    [NotMapped]
    public string? NombreCliente { get; set; }

    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string FullPhone => $"({CodeCountry}) - ({CodeNumber}) - {PhoneNumber}";

    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string SMSPhone => $"{CodeCountry}{CodeNumber}{PhoneNumber}";

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public Contractor? Contractor { get; set; }
    public ServiceCategory? ServiceCategory { get; set; }
    public ServiceClient? ServiceClient { get; set; }
    public Client? Client { get; set; }
    public Zone? Zone { get; set; }

    public ICollection<ContractIp>? ContractIps { get; set; }
    public ICollection<ContractServer>? ContractServers { get; set; }
    public ICollection<ContractPlan>? ContractPlans { get; set; }
    public ICollection<ContractNode>? ContractNodes { get; set; }
    public ICollection<ContractQue>? ContractQues { get; set; }

}