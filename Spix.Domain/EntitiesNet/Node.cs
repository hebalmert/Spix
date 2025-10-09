using Spix.Core.EntitiesContratos;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesData;
using Spix.Domain.EntitiesGen;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Core.EntitiesNet;

public class Node
{
    private string? _mac;

    public Guid NodeId { get; set; }

    [Display(Name = "Nodo/SSID")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NodesName { get; set; } = null!;

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [DisplayName("Ip Network")]
    public Guid IpNetworkId { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [DisplayName("Operacion")]
    public int OperationId { get; set; }

    [MaxLength(17, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", ErrorMessage = "Formato incorrecto. Ejemplo válido: 00:1A:2B:3C:4D:5E")]
    [Display(Name = "MAC")]
    public string? Mac
    {
        get => _mac;
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                var cleanedMac = value.Replace(":", "").Replace("-", "").ToUpper();
                if (cleanedMac.Length == 12)
                {
                    _mac = string.Join(":", Enumerable.Range(0, 6).Select(i => cleanedMac.Substring(i * 2, 2)));
                }
                else
                {
                    _mac = value; // Si no tiene la longitud correcta, se deja como está
                }
            }
            else
            {
                _mac = null;
            }
        }
    }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [DisplayName("Marca")]
    public Guid MarkId { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [DisplayName("Modelo")]
    public Guid MarkModelId { get; set; }

    [Display(Name = "Usuario")]
    [MaxLength(25, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Usuario { get; set; } = null!;

    [Display(Name = "Clave")]
    [MaxLength(25, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Clave { get; set; } = null!;

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [DisplayName("Zona")]
    public Guid ZoneId { get; set; }

    [DisplayName("Frecuencia")]
    public int? FrecuencyTypeId { get; set; }

    [DisplayName("Tipo Frecuencia")]
    public int? FrecuencyId { get; set; }

    [DisplayName("Canal")]
    public int? ChannelId { get; set; }

    [DisplayName("Seguridad")]
    public int? SecurityId { get; set; }

    [Display(Name = "Frase")]
    [MaxLength(25, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
    public string? FraseSeguridad { get; set; }

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

    public Operation? Operation { get; set; }

    public Mark? Mark { get; set; }

    public MarkModel? MarkModel { get; set; }

    public Zone? Zone { get; set; }

    public FrecuencyType? FrecuencyType { get; set; }

    public Frecuency? Frecuency { get; set; }

    public Channel? Channel { get; set; }

    public Security? Security { get; set; }

    //Relaciones en dos vias

    public ICollection<ContractNode>? ContractNodes { get; set; }
}