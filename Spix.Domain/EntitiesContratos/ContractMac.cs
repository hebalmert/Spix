using Spix.Domain.EntitiesInven;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class ContractMac
{
    [Key]
    public Guid ContractMacId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = "Contrato")]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = "Mac Cliente")]
    public Guid CargueDetailId { get; set; }

    public virtual ContractClient? ContractClient { get; set; }

    public virtual CargueDetail? CargueDetail { get; set; }
}
