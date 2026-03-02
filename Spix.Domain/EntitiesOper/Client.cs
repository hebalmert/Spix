using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesOper;

public class Client
{
    [Key]
    public Guid ClientId { get; set; }

    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = nameof(Resource.Created), ResourceType = typeof(Resource))]
    public DateTime? DateCreated { get; set; }

    [Display(Name = nameof(Resource.DocumentType), ResourceType = typeof(Resource))]
    public Guid DocumentTypeId { get; set; }

    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Document), ResourceType = typeof(Resource))]
    public string Document { get; set; } = null!;

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.FirstName), ResourceType = typeof(Resource))]
    public string FirstName { get; set; } = null!;

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.LastName), ResourceType = typeof(Resource))]
    public string LastName { get; set; } = null!;

    [MaxLength(101, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
    public string? FullName { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [MaxLength(7, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = nameof(Resource.Country), ResourceType = typeof(Resource))]
    public string CodeCountry { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [MaxLength(3, ErrorMessage = "El {0} no puede tener mas de {1} Caracteres.")]
    [Display(Name = nameof(Resource.Code), ResourceType = typeof(Resource))]
    public string CodeNumber { get; set; } = null!;

    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string PhoneNumber { get; set; } = null!;

    [MaxLength(256, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Address), ResourceType = typeof(Resource))]
    public string Address { get; set; } = null!;

    [MaxLength(256, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.EmailAddress)]
    [Display(Name = nameof(Resource.Email), ResourceType = typeof(Resource))]
    public string Email { get; set; } = null!;

    [MaxLength(24)]
    [StringLength(24, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessageResourceName = nameof(Resource.Validation_UserNameFormat), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.UserName), ResourceType = typeof(Resource))]
    public string UserName { get; set; } = null!;

    [Display(Name = nameof(Resource.UserType), ResourceType = typeof(Resource))]
    public UserType UserType { get; set; }

    [Display(Name = nameof(Resource.Photo), ResourceType = typeof(Resource))]
    public string? Imagen { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [NotMapped]
    public string? ImageFullPath { get; set; }

    [NotMapped]
    public string? ImgBase64 { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public DocumentType? DocumentType { get; set; }
    public ICollection<ContractClient>? ContractClients { get; set; }

}