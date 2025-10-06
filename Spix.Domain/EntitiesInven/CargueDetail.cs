using Spix.Domain.Entities;
using Spix.Domain.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesInven;

public class CargueDetail
{
    private string? _mac;

    [Key]
    public Guid CargueDetailId { get; set; }

    [Required(ErrorMessage = "La {0} es Obligatorio")]
    [DisplayName("Cargue")]
    public Guid CargueId { get; set; }

    [Required(ErrorMessage = "El Campo {0} es Requerido")]
    [MaxLength(17, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", ErrorMessage = "Formato incorrecto. Ejemplo válido: 00:1A:2B:3C:4D:5E")]
    [Display(Name = "MAC")]
    public string? MacWlan
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

    //Fecha de la ultima operacion que se le hizo al equipo, sea que haya cambiado de Active a EnUso o Averiado o vuelto a EnUso
    [Display(Name = "Fecha")]
    public DateTime? DateCargue { get; set; }

    [MaxLength(200, ErrorMessage = "El Maximo de caracteres es {0}")]
    [Display(Name = "Comentario")]
    public string? Comment { get; set; }

    [Display(Name = "Estado")]
    public SerialStateType Status { get; set; } = SerialStateType.Disponible;

    //A que Corporacion Pertenece
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Cargue? Cargue { get; set; }
}