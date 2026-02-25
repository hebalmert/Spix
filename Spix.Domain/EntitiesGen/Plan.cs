using Spix.Core.EntitiesContratos;
using Spix.Domain.Entities;
using Spix.Domain.Enum;
using Spix.Domain.Resources;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class Plan
{
    [Key]
    public Guid PlanId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [DisplayName("Categoria")]
    public Guid PlanCategoryId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Plan")]
    public string PlanName { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Range(1, double.MaxValue, ErrorMessage = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "UpLoad")]
    public int? SpeedUp { get; set; }

    [Display(Name = "Medida")]
    public SpeedUpType SpeedUpType { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Range(1, double.MaxValue, ErrorMessage = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Download")]
    public int? SpeedDown { get; set; }

    [Display(Name = "Medida")]
    public SpeedDownType SpeedDownType { get; set; }

    [Range(1, 12, ErrorMessage = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Reuso 1 a 12")]
    public int? TasaReuso { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Impuesto")]
    public Guid TaxId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Range(1, double.MaxValue, ErrorMessage = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Display(Name = "Precio Venta Sin Iva")]
    public decimal? Price { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public PlanCategory? PlanCategory { get; set; }

    public Tax? Tax { get; set; }

    //Relaciones en dos vias
    public ICollection<ContractPlan>? ContractPlans { get; set; }

    public ICollection<ContractQue>? ContractQues { get; set; }
}