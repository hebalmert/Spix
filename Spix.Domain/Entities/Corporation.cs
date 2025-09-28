using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Resources;

namespace Spix.Domain.Entities;

public class Corporation
{
    [Key]
    public int CorporationId { get; set; }

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Corporation")]
    public string? Name { get; set; }

    [MaxLength(5, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Type Document")]
    public string? TypeDocument { get; set; }

    [MaxLength(15, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Document")]
    public string? NroDocument { get; set; }

    [MaxLength(12, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [MaxLength(200, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Required]
    [Display(Name = "Country")]
    public int CountryId { get; set; }

    [Required]
    [Display(Name = "Soft Plan")]
    public int SoftPlanId { get; set; }

    //Tiempo Activo de la cuenta
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Start")]
    public DateTime DateStart { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "End")]
    public DateTime DateEnd { get; set; }

    [Display(Name = "Logo")]
    public string? Imagen { get; set; }

    [Display(Name = "Active")]
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