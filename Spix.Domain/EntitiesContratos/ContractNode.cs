using Spix.Core.EntitiesNet;
using System.ComponentModel.DataAnnotations;

namespace Spix.Core.EntitiesContratos;

public class ContractNode
{
    [Key]
    public Guid ContractNodeId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = "Contrato")]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "AP Cliente")]
    public Guid NodeId { get; set; }

    public virtual ContractClient? ContractClient { get; set; }

    public virtual Node? Node { get; set; }
}