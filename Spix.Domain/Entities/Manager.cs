using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.Entities;

public class Manager
{
    [Key]
    public int ManagerId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.FirstName), ResourceType = typeof(Resource))]
    public string FirstName { get; set; } = null!;

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.LastName), ResourceType = typeof(Resource))]
    public string LastName { get; set; } = null!;


    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Document), ResourceType = typeof(Resource))]
    public string? NroDocument { get; set; }

    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string PhoneNumber { get; set; } = null!;

    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Address), ResourceType = typeof(Resource))]
    public string Address { get; set; } = null!;

    [MaxLength(256, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Email), ResourceType = typeof(Resource))]
    public string Email { get; set; } = null!;

    //Usuario para el Logueo
    [StringLength(24, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessageResourceName = nameof(Resource.Validation_InvalidFormat), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.UserName), ResourceType = typeof(Resource))]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Corporation), ResourceType = typeof(Resource))]
    public int CorporationId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.JobPosition), ResourceType = typeof(Resource))]
    public string Job { get; set; } = null!;

    [Display(Name = nameof(Resource.UserType), ResourceType = typeof(Resource))]
    public UserType UserType { get; set; }

    [Display(Name = nameof(Resource.Photo), ResourceType = typeof(Resource))]
    public string? Imagen { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    //TODO: Cambio de ruta para Imagenes
    [NotMapped]
    public string? ImageFullPath { get; set; }

    [NotMapped]
    public string? ImgBase64 { get; set; }

    //Relaciones
    public Corporation? Corporation { get; set; }
}