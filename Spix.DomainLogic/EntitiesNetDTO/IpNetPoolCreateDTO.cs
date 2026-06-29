using System.ComponentModel.DataAnnotations;
using Spix.xLanguage.Resources;

namespace Spix.DomainLogic.EntitiesNetDTO;

public class IpNetPoolCreateDTO
{
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){2}[0-9]{1,3}$", ErrorMessage = "El Campo {0} debe tener el Formato Ejm: 192.168.0")]
    public string? IpAddress { get; set; }

    [Range(0, 255)]
    public int Desde { get; set; }

    [Range(0, 255)]
    public int Hasta { get; set; }
}
