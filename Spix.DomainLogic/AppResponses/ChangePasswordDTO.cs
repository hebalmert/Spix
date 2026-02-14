using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.DomainLogic.AppResponses;

public class ChangePasswordDTO
{
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.PasswordCurrent), ResourceType = typeof(Resource))]
    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = null!;

    [Compare("NewPassword", ErrorMessageResourceName = nameof(Resource.Validation_PasswordMismatch), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.PasswordConfirm), ResourceType = typeof(Resource))]
    public string Confirm { get; set; } = null!;
}