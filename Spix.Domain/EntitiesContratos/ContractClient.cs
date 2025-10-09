using Spix.Core.EntitiesOper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Core.EntitiesContratos;

public class ContractClient
{
    [Key]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = false)]
    [Display(Name = "Fecha")]
    public DateTime DateCreado { get; set; }

    [Display(Name = "# Contrato")]
    public string? ControlContrato { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = "Contratista")]
    public Guid ContractorId { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = "Cliente")]
    public Guid ClientId { get; set; }

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(7, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Pais")]
    public string CodeCountry { get; set; } = null!;

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(3, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Codigo")]
    public string CodeNumber { get; set; } = null!;

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(7, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = "Telefono")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "El {0} es Obligatorio")]
    [MaxLength(256, ErrorMessage = "El campo no puede ser mayor a {0} de largo")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Direccion")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = "Zona")]
    public Guid ZoneId { get; set; }

    [Display(Name = "Estado")]
    public StateType StateType { get; set; }

    [Display(Name = "Antena de Empresa")]
    public bool EquipoEmpres { get; set; }

    [Display(Name = "Invoice Cliente")]
    public bool EnvoiceClient { get; set; }

    //Costo de Instalacion del Servicio
    //..

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = "Categoria Servicio")]
    public Guid ServiceCategoryId { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = "Servicio Cliente")]
    public Guid ServiceClientId { get; set; }

    [MaxLength(50, ErrorMessage = "El Maximo de caracteres es {0}")]
    //[Required(ErrorMessage = "El campo {0} es Requerido")]
    [Display(Name = "Servicio")]
    public string? ServiceName { get; set; }

    //[Required(ErrorMessage = "El campo {0} es Requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El Valor del Precio debe ser mayor que {1}")]
    [Display(Name = "Impuesto")]
    public decimal? Impuesto { get; set; }

    //[Required(ErrorMessage = "El campo {0} es Requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El Valor del Precio debe ser mayor que {1}")]
    [Display(Name = "Precio")]
    public decimal? Price { get; set; }

    //Pago del Contrato de Intalacion
    //..

    [DisplayName("Usa Control HotSpot")]
    [NotMapped]
    public bool UsaHotSpotControl { get; set; }

    [DisplayName("Estado")]
    [NotMapped]
    public int StateId { get; set; }

    [DisplayName("Ciudad")]
    [NotMapped]
    public int CityId { get; set; }

    [DisplayName("Cliente")]
    [NotMapped]
    public string? NombreCliente { get; set; }

    //Propiedades Virtuales

    [DisplayName("Telefono")]
    public string FullPhone => $"({CodeCountry}) - ({CodeNumber}) - {PhoneNumber}";

    [DisplayName("Telefono")]
    public string SMSPhone => $"{CodeCountry}{CodeNumber}{PhoneNumber}";

    //Propiedad de Relaciones entre las Entidades
    //...
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Contractor? Contractor { get; set; }

    public ServiceCategory? ServiceCategory { get; set; }

    public ServiceClient? ServiceClient { get; set; }

    public Client? Client { get; set; }

    public Zone? Zone { get; set; }

    //Relaciones en dos vias

    public ICollection<ContractIp>? ContractIps { get; set; }
    public ICollection<ContractServer>? ContractServers { get; set; }
    public ICollection<ContractPlan>? ContractPlans { get; set; }
    public ICollection<ContractNode>? ContractNodes { get; set; }
    public ICollection<ContractQue>? ContractQues { get; set; }
}