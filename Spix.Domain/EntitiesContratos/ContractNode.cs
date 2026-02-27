using Spix.Domain.EntitiesNet;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class ContractNode
{
    [Key]
    public Guid ContractNodeId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = nameof(Resource.Contract), ResourceType = typeof(Resource))]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.ClientAP), ResourceType = typeof(Resource))]
    public Guid NodeId { get; set; }

    public ContractClient? ContractClient { get; set; }
    public Node? Node { get; set; }

}