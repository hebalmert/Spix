using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesData;
using Spix.Domain.EntitiesGen;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesNet;

public class Node
{
    private string? _mac;
    private decimal? _latitude;
    private decimal? _longitude;

    [Key]
    public Guid NodeId { get; set; }

    [Display(Name = nameof(Resource.NodeSSID), ResourceType = typeof(Resource))]
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public string NodesName { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.IpNetwork), ResourceType = typeof(Resource))]
    public Guid IpNetworkId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Operation), ResourceType = typeof(Resource))]
    public int OperationId { get; set; }

    [MaxLength(17, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", ErrorMessage = "Formato incorrecto. Ejemplo válido: 00:1A:2B:3C:4D:5E")]
    [Display(Name = nameof(Resource.MAC), ResourceType = typeof(Resource))]
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
                    _mac = value;
                }
            }
            else
            {
                _mac = null;
            }
        }
    }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Mark), ResourceType = typeof(Resource))]
    public Guid MarkId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Model), ResourceType = typeof(Resource))]
    public Guid MarkModelId { get; set; }

    [Display(Name = nameof(Resource.User), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public string Usuario { get; set; } = null!;

    [Display(Name = nameof(Resource.Password), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public string Clave { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Zone), ResourceType = typeof(Resource))]
    public Guid ZoneId { get; set; }

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
                // Validar rango
                if (value.Value < -90 || value.Value > 90)
                {
                    throw new ArgumentOutOfRangeException(nameof(Latitude), "Latitud debe estar entre -90 y 90");
                }

                // Redondear a 7 decimales y validar formato
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
                // Validar rango
                if (value.Value < -180 || value.Value > 180)
                {
                    throw new ArgumentOutOfRangeException(nameof(Longitude), "Longitud debe estar entre -180 y 180");
                }

                // Redondear a 7 decimales y validar formato
                _longitude = Math.Round(value.Value, 7);
            }
            else
            {
                _longitude = null;
            }
        }
    }

    [Display(Name = nameof(Resource.FrequencyType), ResourceType = typeof(Resource))]
    public int? FrecuencyTypeId { get; set; }

    [Display(Name = nameof(Resource.Frequency), ResourceType = typeof(Resource))]
    public int? FrecuencyId { get; set; }

    [Display(Name = nameof(Resource.Channel), ResourceType = typeof(Resource))]
    public int? ChannelId { get; set; }

    [Display(Name = nameof(Resource.Security), ResourceType = typeof(Resource))]
    public int? SecurityId { get; set; }

    [Display(Name = nameof(Resource.SecurityPhrase), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    public string? FraseSeguridad { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [NotMapped]
    public int StateId { get; set; }

    [NotMapped]
    public int CityId { get; set; }

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

    public ICollection<ContractNode>? ContractNodes { get; set; }

}