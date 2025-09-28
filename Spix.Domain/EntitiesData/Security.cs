using Spix.Domain.EntitiesNet;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesData;

public class Security
{
    [Key]
    public int SecurityId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Tipo Seguridad")]
    public string SecurityName { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Relaciones

    public ICollection<Node>? Nodes { get; set; }
}