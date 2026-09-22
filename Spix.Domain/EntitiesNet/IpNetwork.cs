using Spix.Domain.Entities;
using Spix.DomainLogic.ModelUtility;
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
            _ip = Normalize(value);
            IpSort = IpSortKey.From(_ip);
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

    //Clave numerica de la IP para ordenar (10.0.0.2 antes que 10.0.0.10).
    //Se calcula sola al asignar Ip y se guarda con indice: el listado no ordena toda la tabla.
    public long? IpSort { get; private set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<Node>? Nodes { get; set; }
    public ICollection<Server>? Servers { get; set; }

    //Limpia lo que escribe el usuario: deja solo digitos y puntos, y si vienen solo digitos arma los cuatro octetos
    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '.').ToArray());

        if (System.Net.IPAddress.TryParse(cleaned, out _))
        {
            return cleaned;
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
                    return candidate;
                }
            }
        }

        return value;
    }
}
