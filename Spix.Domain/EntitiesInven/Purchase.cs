using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class Purchase
{
    [Key]
    public Guid PurchaseId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.PurchaseDate), ResourceType = typeof(Resource))]
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.PurchaseNumber), ResourceType = typeof(Resource))]
    public int NroPurchase { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Supplier), ResourceType = typeof(Resource))]
    public Guid SupplierId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Storage), ResourceType = typeof(Resource))]
    public Guid ProductStorageId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.InvoiceDate), ResourceType = typeof(Resource))]
    public DateTime FacuraDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.InvoiceNumber), ResourceType = typeof(Resource))]
    public string NroFactura { get; set; } = null!;

    [Display(Name = nameof(Resource.Status), ResourceType = typeof(Resource))]
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pendiente;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Subtotal), ResourceType = typeof(Resource))]
    public decimal SubTotalCompra => PurchaseDetails == null ? 0 : PurchaseDetails.Sum(x => x.SubTotal);

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Tax), ResourceType = typeof(Resource))]
    public decimal ImpuestoTotalCompra => PurchaseDetails == null ? 0 : PurchaseDetails.Sum(x => x.Impuesto);

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = nameof(Resource.Total), ResourceType = typeof(Resource))]
    public decimal TotalCompra => PurchaseDetails == null ? 0 : PurchaseDetails.Sum(x => x.TotalGeneral);

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public Supplier? Supplier { get; set; }
    public ProductStorage? ProductStorage { get; set; }
    public ICollection<PurchaseDetail>? PurchaseDetails { get; set; }
    public ICollection<Cargue>? Cargue { get; set; }

}