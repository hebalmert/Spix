using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.DomainLogic.AppResponses;

public class ResetPasswordDTO
{
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [EmailAddress(ErrorMessageResourceName = nameof(Resource.Validation_InvalidEmail), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [EmailAddress(ErrorMessageResourceName = nameof(Resource.Validation_InvalidEmail), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "User Name")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = null!;

    [Compare("NewPassword", ErrorMessageResourceName = nameof(Resource.Validation_PasswordMismatch), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = null!;

    public string Token { get; set; } = null!;
}