using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class Cargue
{
    [Key]
    public Guid CargueId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = false)]
    [Display(Name = nameof(Resource.Date), ResourceType = typeof(Resource))]
    public DateTime DateCargue { get; set; }

    [Display(Name = nameof(Resource.CargueNro), ResourceType = typeof(Resource))]
    public string? ControlCargue { get; set; }

    [Display(Name = nameof(Resource.Invoice), ResourceType = typeof(Resource))]
    [MaxLength(20, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public Guid PurchaseId { get; set; }

    [Display(Name = nameof(Resource.Invoice), ResourceType = typeof(Resource))]
    [MaxLength(20, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public Guid PurchaseDetailId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Quantity), ResourceType = typeof(Resource))]
    public decimal CantToUp { get; set; }

    [Display(Name = nameof(Resource.Product), ResourceType = typeof(Resource))]
    public Guid ProductId { get; set; }

    [Display(Name = nameof(Resource.Status), ResourceType = typeof(Resource))]
    public CargueType Status { get; set; } = CargueType.Pendiente;

    [Display(Name = nameof(Resource.Serials), ResourceType = typeof(Resource))]
    public int TotalSeriales => CargueDetails == null ? 0 : CargueDetails.Count();

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public Purchase? Purchase { get; set; }
    public PurchaseDetail? PurchaseDetail { get; set; }
    public Product? Product { get; set; }

    public ICollection<CargueDetail>? CargueDetails { get; set; }

}