using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.Validations;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

//Por que OLT entra este contrato. Es el gemelo de ContractNode: informativo, igual que el
//nodo, y se usa en los de FIBRA. Un contrato tiene nodo (inalambrico) u OLT (fibra), no los
//dos: lo decide el control del servidor.
public class ContractOlt
{
    [Key]
    public Guid ContractOltId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contract), ResourceType = typeof(Resource))]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Olt), ResourceType = typeof(Resource))]
    [ComboRequired]
    public Guid OltId { get; set; }

    public ContractClient? ContractClient { get; set; }
    public Olt? Olt { get; set; }
}
