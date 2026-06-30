using Spix.Domain.EntitiesInven;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class ContractMac
{
    [Key]
    public Guid ContractMacId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contract), ResourceType = typeof(Resource))]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.MAC), ResourceType = typeof(Resource))]
    public Guid CargueDetailId { get; set; }

    public virtual ContractClient? ContractClient { get; set; }

    public virtual CargueDetail? CargueDetail { get; set; }
}
