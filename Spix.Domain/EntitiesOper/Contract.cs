using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spi.Shared.EntitiesSoft;

public class Contract
{
    [Key]
    public int ContractId { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = false)]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Date), ResourceType = typeof(Resource))]
    public DateTime DateCreado { get; set; }

    [Display(Name = nameof(Resource.ContractNumber), ResourceType = typeof(Resource))]
    public string? ControlContrato { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contractor), ResourceType = typeof(Resource))]
    public int ContractorId { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Client), ResourceType = typeof(Resource))]
    public int ClientId { get; set; }

    [MaxLength(7, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Country), ResourceType = typeof(Resource))]
    public string CodeCountry { get; set; } = null!;

    [MaxLength(3, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Code), ResourceType = typeof(Resource))]
    public string CodeNumber { get; set; } = null!;

    [MaxLength(7, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [MaxLength(256, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.MultilineText)]
    [Display(Name = nameof(Resource.Address), ResourceType = typeof(Resource))]
    public string Address { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = nameof(Resource.Zone), ResourceType = typeof(Resource))]
    public int ZoneId { get; set; }

    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    public StateType StateType { get; set; }

    [Display(Name = nameof(Resource.CompanyAntenna), ResourceType = typeof(Resource))]
    public bool EquipoEmpres { get; set; }

    [Display(Name = nameof(Resource.InvoiceClient), ResourceType = typeof(Resource))]
    public bool EnvoiceClient { get; set; }

    [MaxLength(50, ErrorMessage = "El Maximo de caracteres es {0}")]
    [Display(Name = nameof(Resource.Service), ResourceType = typeof(Resource))]
    public string? ServiceName { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El Valor del Precio debe ser mayor que {1}")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Tax), ResourceType = typeof(Resource))]
    public decimal? Impuesto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El Valor del Precio debe ser mayor que {1}")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Price), ResourceType = typeof(Resource))]
    public decimal? Precio1 { get; set; }

    [Display(Name = nameof(Resource.UseHotSpotControl), ResourceType = typeof(Resource))]
    [NotMapped]
    public bool UsaHotSpotControl { get; set; }

    [NotMapped]
    public int StateId { get; set; }

    [NotMapped]
    public int CityId { get; set; }

    [Display(Name = nameof(Resource.Client), ResourceType = typeof(Resource))]
    [NotMapped]
    public string? NombreCliente { get; set; }

    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string FullPhone => $"({CodeCountry}) - ({CodeNumber}) - {PhoneNumber}";

    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string SMSPhone => $"{CodeCountry}{CodeNumber}{PhoneNumber}";

    public int CorporateId { get; set; }

    public Corporation? Corporate { get; set; }
    public Contractor? Contractor { get; set; }
    public Client? Client { get; set; }
    public Zone? Zone { get; set; }


    //public ICollection<CxCBill>? CxCBill { get; set; }

    //public ICollection<ContractIp>? ContractIps { get; set; } = new List<ContractIp>();

    //public ICollection<ContractMac>? ContractMacs { get; set; } = new List<ContractMac>();

    //public ICollection<ContractServer>? ContractServers { get; set; } = new List<ContractServer>();

    //public ICollection<ContractPlan>? ContractPlans { get; set; } = new List<ContractPlan>();

    //public ICollection<ContractNode>? ContractNodes { get; set; } = new List<ContractNode>();

    //public ICollection<ContractQue>? ContractQues { get; set; } = new List<ContractQue>();

    //public ICollection<ContractBind>? ContractBinds { get; set; } = new List<ContractBind>();

    //public ICollection<ContractWithdrawal>? ContractWithdrawals { get; set; }

    //public ICollection<ServiceRequest>? ServiceRequests { get; set; }

    //public ICollection<PrePayment>? PrePayments { get; set; }

    //public ICollection<ContractSuspended>? ContractSuspendeds { get; set; }

    //public ICollection<BillingNoteOne>? BillingNoteOnes { get; set; }

    //public ICollection<ContractCutDetail>? ContractCutDetails { get; set; }

    //public ICollection<PreExonerated>? PreExonerateds { get; set; }

    //public ICollection<PaymentCachier>? PaymentCachiers { get; set; }

    //public ICollection<PaymentAuxUser>? PaymentAuxUsers { get; set; }

    //public ICollection<PrepaymentCachier>? PrepaymentCachiers { get; set; }

    //public ICollection<PrePaymentAuxUser>? PrePaymentAuxUsers { get; set; }

    //public ICollection<ContractorPay>? ContractorPays { get; set; }
}