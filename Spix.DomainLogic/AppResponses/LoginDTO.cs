using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.DomainLogic.AppResponses;

public class LoginDTO
{
    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [RegularExpression(@"^(?!.*\.\.)(?!\.)[a-zA-Z0-9.]+(?<!\.)$", ErrorMessageResourceName = nameof(Resource.Validation_UserNameFormat), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.UserName), ResourceType = typeof(Resource))]
    public string UserName { get; set; } = null!;

    [StringLength(20, MinimumLength = 6, ErrorMessageResourceName = nameof(Resource.Validation_StringLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Password), ResourceType = typeof(Resource))]
    public string Password { get; set; } = null!;
}