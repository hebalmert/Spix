using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesInven;

public class CargueDetail
{
    private string? _mac;

    [Key]
    public Guid CargueDetailId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Cargue), ResourceType = typeof(Resource))]
    public Guid CargueId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [MaxLength(17, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", ErrorMessage = "Formato incorrecto. Ejemplo válido: 00:1A:2B:3C:4D:5E")]
    [Display(Name = nameof(Resource.MAC), ResourceType = typeof(Resource))]
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
                    _mac = value;
                }
            }
            else
            {
                _mac = null;
            }
        }
    }

    [Display(Name = nameof(Resource.Date), ResourceType = typeof(Resource))]
    public DateTime? DateCargue { get; set; }

    [MaxLength(200, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Comment), ResourceType = typeof(Resource))]
    public string? Comment { get; set; }

    [Display(Name = nameof(Resource.Status), ResourceType = typeof(Resource))]
    public SerialStateType Status { get; set; } = SerialStateType.Disponible;

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public Cargue? Cargue { get; set; }

}