using Spix.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Spix.Core.EntitiesNet;

public class IpNetwork
{
    public Guid IpNetworkId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [MaxLength(50, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "El Campo {0} tener el Formato Ejm: 192.168.0.0")]
    [Display(Name = "IP Address")]
    public string? Ip { get; set; }

    [MaxLength(250, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Detalle")]
    public string? Description { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    [Display(Name = "Asignada")]
    public bool Assigned { get; set; }

    [Display(Name = "Exonerada")]
    public bool Excluded { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<Node>? Nodes { get; set; }

    public ICollection<Server>? Servers { get; set; }
}