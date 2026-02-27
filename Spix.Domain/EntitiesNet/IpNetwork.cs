using Spix.Domain.Entities;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesNet;

public class IpNetwork
{
    private string? _ip;

    [Key]
    public Guid IpNetworkId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "El Campo {0} tener el Formato Ejm: 192.168.0.0")]
    [Display(Name = nameof(Resource.IPAddress), ResourceType = typeof(Resource))]
    public string? Ip
    {
        get => _ip;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _ip = null;
                return;
            }

            var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '.').ToArray());

            if (System.Net.IPAddress.TryParse(cleaned, out _))
            {
                _ip = cleaned;
                return;
            }

            var digits = new string(cleaned.Where(char.IsDigit).ToArray());

            if (digits.Length <= 12)
            {
                var segments = new List<string>();
                int index = 0;

                while (index < digits.Length && segments.Count < 4)
                {
                    int remaining = digits.Length - index;
                    int take = Math.Min(3, remaining);
                    segments.Add(digits.Substring(index, take));
                    index += take;
                }

                if (segments.Count == 4)
                {
                    var candidate = string.Join(".", segments);

                    if (System.Net.IPAddress.TryParse(candidate, out _))
                    {
                        _ip = candidate;
                        return;
                    }
                }
            }

            _ip = value;
        }
    }

    [MaxLength(250, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [DataType(DataType.MultilineText)]
    [Display(Name = nameof(Resource.Detail), ResourceType = typeof(Resource))]
    public string? Description { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [Display(Name = nameof(Resource.Assigned), ResourceType = typeof(Resource))]
    public bool Assigned { get; set; }

    [Display(Name = nameof(Resource.Excluded), ResourceType = typeof(Resource))]
    public bool Excluded { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<Node>? Nodes { get; set; }
    public ICollection<Server>? Servers { get; set; }

}