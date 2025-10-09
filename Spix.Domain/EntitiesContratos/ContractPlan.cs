using Spix.Domain.EntitiesGen;
using System.ComponentModel.DataAnnotations;

namespace Spix.Core.EntitiesContratos;

public class ContractPlan
{
    [Key]
    public int ContractPlanId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Contrato")]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Plan Cliente")]
    public Guid PlanId { get; set; }

    public virtual ContractClient? ContractClient { get; set; }

    public virtual Plan? Plan { get; set; }
}