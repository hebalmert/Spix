using System.ComponentModel.DataAnnotations;
using Spix.Domain.Resources;

namespace Spix.DomainLogic.ResponcesSec;

public class ChangePasswordDTO
{
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_BetweenLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = null!;

    [DataType(DataType.Password)]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_BetweenLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = null!;

    [Compare("NewPassword", ErrorMessageResourceName = nameof(Resource.Validation_PasswordMismatch), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_BetweenLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; } = null!;
}