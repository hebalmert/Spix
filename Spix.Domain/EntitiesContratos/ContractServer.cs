using Spix.Domain.EntitiesNet;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class ContractServer
{
    public Guid ContractServerId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contract), ResourceType = typeof(Resource))]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Server), ResourceType = typeof(Resource))]
    public Guid ServerId { get; set; }

    public ContractClient? ContractClient { get; set; }
    public Server? Server { get; set; }

}