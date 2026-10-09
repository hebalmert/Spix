using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Spix.DomainLogic.Validations;

namespace Spix.Domain.EntitiesInven;

public class Supplier
{
    [Key]
    public Guid SupplierId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.SupplierName), ResourceType = typeof(Resource))]
    public string Name { get; set; } = null!;

    [Display(Name = nameof(Resource.DocumentType), ResourceType = typeof(Resource))]
    [ComboRequired]
    public Guid DocumentTypeId { get; set; }

    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Document), ResourceType = typeof(Resource))]
    public string Document { get; set; } = null!;

    //El indicativo del pais, escrito a mano: +57, +58. En una misma empresa hay
    //clientes de paises distintos, asi que se guarda por persona.
    [MaxLength(6, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.PhoneCode), ResourceType = typeof(Resource))]
    public string CodeCountry { get; set; } = null!;

    //El indicativo del operador: 300, 318.
    [MaxLength(6, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.PhoneArea), ResourceType = typeof(Resource))]
    public string CodeNumber { get; set; } = null!;

    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string PhoneNumber { get; set; } = null!;

    [MaxLength(200, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Address), ResourceType = typeof(Resource))]
    public string? Address { get; set; }

    [MaxLength(256, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.EmailAddress)]
    [Display(Name = nameof(Resource.Email), ResourceType = typeof(Resource))]
    public string Email { get; set; } = null!;

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contact), ResourceType = typeof(Resource))]
    public string? ContactName { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.State), ResourceType = typeof(Resource))]
    [ComboRequired]
    public int StateId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.City), ResourceType = typeof(Resource))]
    [ComboRequired]
    public int CityId { get; set; }

    [Display(Name = nameof(Resource.Photo), ResourceType = typeof(Resource))]
    public string? Photo { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [NotMapped]
    public string? ImageFullPath { get; set; }

    [NotMapped]
    public string? ImgBase64 { get; set; }

    public int CorporationId { get; set; }
    public Corporation? Corporation { get; set; }

    public State? State { get; set; }
    public City? City { get; set; }
    public DocumentType? DocumentType { get; set; }
    public ICollection<Purchase>? Purchases { get; set; }

}