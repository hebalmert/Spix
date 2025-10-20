using Microsoft.AspNetCore.Identity;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.Entities;

public class User : IdentityUser
{
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Firt Name")]
    public string FirstName { get; set; } = null!;

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;

    [MaxLength(101, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Full Name")]
    public string? FullName { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Job Position")]
    public string JobPosition { get; set; } = null!;

    //Identificacion de Origenes y Role del Usuario
    [Display(Name = "Origen")]
    public string? UserFrom { get; set; }

    [Display(Name = "Photo User")]
    public string? PhotoUser { get; set; }

    [Display(Name = "Active")]
    public bool Active { get; set; }

    [NotMapped]
    public string? Pass { get; set; }

    public int? CorporationId { get; set; }

    //Relaciones

    public Corporation? Corporation { get; set; }
    public ICollection<UserRoleDetails>? UserRoleDetails { get; set; }
    public ICollection<Transfer>? Transfers { get; set; }
}