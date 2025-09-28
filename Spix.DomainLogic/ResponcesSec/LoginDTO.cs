using System.ComponentModel.DataAnnotations;
using Spix.Domain.Resources;

namespace Spix.DomainLogic.ResponcesSec;

public class LoginDTO
{
    [StringLength(24, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [RegularExpression(@"^(?!.*\.\.)(?!\.)[a-zA-Z0-9.]+(?<!\.)$", ErrorMessageResourceName = nameof(Resource.Validation_UserNameFormat), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "User ID")]
    public string UserName { get; set; } = null!;

    [MinLength(6, ErrorMessageResourceName = nameof(Resource.Validation_MinLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;
}