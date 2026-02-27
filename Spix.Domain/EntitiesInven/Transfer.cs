using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesInven;

public class Transfer
{
    [Key]
    public Guid TransferId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.TransferDate), ResourceType = typeof(Resource))]
    public DateTime DateTransfer { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.TransferNumber), ResourceType = typeof(Resource))]
    public int NroTransfer { get; set; }

    [Display(Name = nameof(Resource.User), ResourceType = typeof(Resource))]
    public string? UserId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.FromStorage), ResourceType = typeof(Resource))]
    public string? FromStorageName { get; set; }

    public Guid FromProductStorageId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.ToStorage), ResourceType = typeof(Resource))]
    public string? ToStorageName { get; set; }

    public Guid ToProductStorageId { get; set; }

    [Display(Name = nameof(Resource.Status), ResourceType = typeof(Resource))]
    public TransferType? Status { get; set; }

    [NotMapped]
    [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
    public string? NombreUsuario { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public User? User { get; set; }
    public ICollection<TransferDetails>? TransferDetails { get; set; }

}