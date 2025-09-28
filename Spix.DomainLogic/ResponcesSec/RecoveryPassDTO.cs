using System.ComponentModel.DataAnnotations;
using Spix.Domain.Resources;

namespace Spix.DomainLogic.ResponcesSec;

public class RecoveryPassDTO
{
    [EmailAddress(ErrorMessageResourceName = nameof(Resource.Validation_InvalidEmail), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "User Name")]
    public string UserName { get; set; } = null!;
}