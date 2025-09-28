using System.ComponentModel.DataAnnotations;
using Spix.Domain.Entities;
using Spix.Domain.Resources;

namespace Spix.Domain.EntitiesSoft;

public class Patient
{
    [Key]
    public Guid PatientId { get; set; }

    [Required]
    [Display(Name = "Frist Name")]
    public Guid PatientControlId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Frist Name")]
    public string FirstName { get; set; } = null!;

    [MaxLength(50, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;

    [MaxLength(101, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Full Name")]
    public string? FullName { get; set; }

    [MaxLength(50, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Prefer Name")]
    public string? Preferido { get; set; }

    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Weight Lb")]
    public double? Weight { get; set; }

    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Height Ft")]
    public string? Height { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Combo), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Sex Birth")]
    public int SexoAsignadoId { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Combo), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Gender Identity ")]
    public int IdentidadGeneroId { get; set; }

    [Required]
    [Display(Name = "DBO")]
    public DateTime DOB { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Combo), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Typo Document")]
    public int DocumentTypeId { get; set; }

    [MaxLength(20, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Document #")]
    public string? Nro_Document { get; set; }

    [MaxLength(256, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [MaxLength(25, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Phone Cell")]
    public string? PhoneCell { get; set; }

    [MaxLength(25, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Phone Home")]
    public string? PhoneHome { get; set; }

    [MaxLength(256, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Combo), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Lenguage")]
    public int IdiomaId { get; set; }

    [Display(Name = "Interpreter")]
    public bool Interpreter { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Combo), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Marital Status")]
    public int EstadoCivilId { get; set; }

    [MaxLength(25, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Ocupacion")]
    public string? Ocupacion { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Combo), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Pharmacy")]
    public int PharmacyId { get; set; }

    //EmergencyContact
    [MaxLength(50, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Full Name")]
    public string? EmgName { get; set; }

    [MaxLength(50, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Relation")]
    public string? EmgRelation { get; set; }

    [MaxLength(25, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Phone")]
    public string? EmgPhone { get; set; }

    [MaxLength(256, ErrorMessageResourceName = "Validation_MaxLength", ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Address")]
    public string? EmgAddress { get; set; }

    //End Emergency Contact

    [Display(Name = "Active")]
    public bool Active { get; set; }

    [Display(Name = "Confirmed")]
    public bool Confirmed { get; set; }

    [Display(Name = "Migrated")]
    public bool Migrated { get; set; }

    //Relaciones

    public int CorporationId { get; set; }
    public Corporation? Corporation { get; set; }
}