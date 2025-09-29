using Spix.Domain.Entities;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class ServiceClient
{
    [Key]
    public Guid ServiceClientId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Categoria")]
    public Guid ServiceCategoryId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Nombre")]
    public string ServiceName { get; set; } = null!;

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Display(Name = "Costo")]
    public decimal Costo { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Impuesto")]
    public Guid TaxId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Display(Name = "Precio Venta Sin Iva")]
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