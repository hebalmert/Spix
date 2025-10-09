using Spix.Core.EntitiesContratos;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Core.EntitiesNet;

public class Server
{
    [Key]
    public Guid ServerId { get; set; }

    [Display(Name = "Servidor")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string ServerName { get; set; } = null!;

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [DisplayName("Ip Network")]
    public Guid IpNetworkId { get; set; }

    [Display(Name = "Usuario")]
    [MaxLength(25, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Usuario { get; set; } = null!;

    [Display(Name = "Clave")]
    [MaxLength(25, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Clave { get; set; } = null!;

    [NotMapped]
    [Display(Name = "Confirmar Clave")]
    [MaxLength(25, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Compare("Clave", ErrorMessage = "La Clave no Coincide, Verifique")]
    public string ClaveConfirm { get; set; } = null!;

    [Display(Name = "Wan Name")]
    [MaxLength(25, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    public string WanName { get; set; } = null!;

    [Display(Name = "Puerto API")]
    public int ApiPort { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = "Marca")]
    public Guid MarkId { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = "Modelo")]
    public Guid MarkModelId { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [Display(Name = "Zona")]
    public Guid ZoneId { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Propiedades No Mapeadas
    [NotMapped]
    public int StateId { get; set; }

    [NotMapped]
    public int CityId { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public IpNetwork? IpNetwork { get; set; }

    public Mark? Mark { get; set; }

    public MarkModel? MarkModel { get; set; }

    public Zone? Zone { get; set; }

    //Relaciones en dos vias
    public ICollection<ContractServer>? ContractServers { get; set; }

    public ICollection<ContractQue>? ContractQues { get; set; }
}