using Spix.Domain.EntitiesData;
using Spix.Domain.EntitiesInven;
using Spix.Domain.EntitiesNet;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class ContractBind
{
    [Key]
    public Guid ContractBindId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = "Contrato")]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = "Servidor")]
    public Guid ServerId { get; set; }


    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = "Ip Cliente")]
    public Guid IpNetId { get; set; }


    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = "Mac Cliente")]
    public Guid CargueDetailId { get; set; }


    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [Range(1, double.MaxValue, ErrorMessage = "Debe Seleccionar un {0}")]
    [Display(Name = "Tipo Acceso")]
    public int HotSpotTypeId { get; set; }


    [MaxLength(15, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    [Display(Name = "Mikrotik Id")]
    public string? MikrotikId { get; set; }

    //Datos que estan guardados en le mikrotik y que se
    //actualizaran si se actualiza el Binding

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
    [Display(Name = "Mac")]
    public string? MacCliente { get; set; }


    public virtual ContractClient? ContractClient { get; set; }

    public virtual Server? Server { get; set; }

    public virtual IpNet? IpNet { get; set; }

    public virtual CargueDetail? CargueDetail { get; set; }

    public virtual HotSpotType? HotSpotType { get; set; }
}
