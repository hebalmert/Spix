using Spix.Domain.EntitesSoftSec;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.Entities;

public class Corporation
{
    [Key]
    public int CorporationId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Corporation), ResourceType = typeof(Resource))]
    public string? Name { get; set; }

    [MaxLength(15, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Document), ResourceType = typeof(Resource))]
    public string? NroDocument { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string? Phone { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Phone), ResourceType = typeof(Resource))]
    public string? Phone2 { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Fax_Number), ResourceType = typeof(Resource))]
    public string? FaxNumber { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Fax_Number), ResourceType = typeof(Resource))]
    public string? FaxNumber2 { get; set; }

    [MaxLength(256, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Address), ResourceType = typeof(Resource))]
    public string? Address { get; set; }

    [Required]
    [Display(Name = nameof(Resource.Country), ResourceType = typeof(Resource))]
    public int CountryId { get; set; }

    [Required]
    [Display(Name = nameof(Resource.Plan), ResourceType = typeof(Resource))]
    public int SoftPlanId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.DateStart), ResourceType = typeof(Resource))]
    public DateTime DateStart { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.DateEnd), ResourceType = typeof(Resource))]
    public DateTime DateEnd { get; set; }

    [Display(Name = nameof(Resource.Logo), ResourceType = typeof(Resource))]
    public string? Imagen { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [NotMapped]
    public string? ImageFullPath { get; set; }

    [NotMapped]
    public string? ImgBase64 { get; set; }

    //Relaciones
    public SoftPlan? SoftPlan { get; set; }
    public Country? Country { get; set; }
    public ICollection<Manager>? Managers { get; set; }
    public ICollection<Usuario>? Usuarios { get; set; }
    public ICollection<UsuarioRole>? UsuarioRoles { get; set; }
}