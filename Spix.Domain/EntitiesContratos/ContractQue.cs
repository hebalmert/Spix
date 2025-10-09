using Spix.Core.EntitiesNet;
using Spix.Domain.EntitiesGen;
using System.ComponentModel.DataAnnotations;

namespace Spix.Core.EntitiesContratos;

public class ContractQue
{
    [Key]
    public Guid ContractQueId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Contrato")]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Servidor")]
    public Guid ServerId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Ip Cliente")]
    public Guid IpNetId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Plan")]
    public Guid PlanId { get; set; }

    //Datos que estan guardados en le mikrotik y que se
    //actualizaran si se actualiza el Queues

    [MaxLength(100)]
    [Display(Name = "Servidor")]
    public string? ServerName { get; set; }

    [MaxLength(100)]
    [Display(Name = "Servidor IP")]
    public string? IpServer { get; set; }

    [MaxLength(100)]
    [Display(Name = "Ip Cliente")]
    public string? IpCliente { get; set; }

    [MaxLength(100)]
    [Display(Name = "Plan")]
    public string? PlanName { get; set; }

    [MaxLength(100)]
    [Display(Name = "Down/Up")]
    public string? TotalVelocidad { get; set; }

    [MaxLength(15, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    [Display(Name = "Mikrotik Id")]
    public string? MikrotikId { get; set; }

    public virtual ContractClient? ContractClient { get; set; }

    public virtual Server? Server { get; set; }

    public virtual Plan? Plan { get; set; }

    public virtual IpNet? IpNet { get; set; }
}