using Spix.Domain.EntitiesNet;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class ContractIp
{
    [Key]
    public Guid ContractIpId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contract), ResourceType = typeof(Resource))]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.ClientIp), ResourceType = typeof(Resource))]
    public Guid IpNetId { get; set; }

    public ContractClient? ContractClient { get; set; }
    public IpNet? IpNet { get; set; }

}