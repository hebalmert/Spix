using Spix.Core.EntitiesNet;
using System.ComponentModel.DataAnnotations;

namespace Spix.Core.EntitiesContratos;

public class ContractIp
{
    [Key]
    public Guid ContractIpId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Contrato")]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Display(Name = "Ip Cliente")]
    public Guid IpNetId { get; set; }

    //Relaciones
    public ContractClient? ContractClient { get; set; }

    public IpNet? IpNet { get; set; }
}