using Spix.Core.EntitiesNet;
using System.ComponentModel.DataAnnotations;

namespace Spix.Core.EntitiesContratos;

public class ContractServer
{
    [Key]
    public Guid ContractServerId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Contrato")]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Servidor")]
    public Guid ServerId { get; set; }

    public virtual ContractClient? ContractClient { get; set; }

    public virtual Server? Server { get; set; }
}