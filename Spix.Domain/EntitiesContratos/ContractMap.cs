using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesContratos;

public class ContractMap
{
    private decimal? _latitude;
    private decimal? _longitude;

    [Key]
    public Guid ContractMapId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contract), ResourceType = typeof(Resource))]
    public Guid ContractClientId { get; set; }

    [Range(-90, 90)]
    [Column(TypeName = "decimal(12,7)")]
    [Display(Name = nameof(Resource.Latitude), ResourceType = typeof(Resource))]
    public decimal? Latitude
    {
        get => _latitude;
        set
        {
            if (value.HasValue)
            {
                if (value.Value < -90 || value.Value > 90)
                    throw new ArgumentOutOfRangeException(nameof(Latitude), "Latitud debe estar entre -90 y 90");

                _latitude = Math.Round(value.Value, 7);
            }
            else
            {
                _latitude = null;
            }
        }
    }

    [Range(-180, 180)]
    [Column(TypeName = "decimal(12,7)")]
    [Display(Name = nameof(Resource.Longitude), ResourceType = typeof(Resource))]
    public decimal? Longitude
    {
        get => _longitude;
        set
        {
            if (value.HasValue)
            {
                if (value.Value < -180 || value.Value > 180)
                    throw new ArgumentOutOfRangeException(nameof(Longitude), "Longitud debe estar entre -180 y 180");

                _longitude = Math.Round(value.Value, 7);
            }
            else
            {
                _longitude = null;
            }
        }
    }

    [NotMapped]
    public string? CoordinatesText { get; set; }

    public ContractClient? ContractClient { get; set; }
}
