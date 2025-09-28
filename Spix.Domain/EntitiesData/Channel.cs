using Spix.Domain.EntitiesNet;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesData;

public class Channel
{
    [Key]
    public int ChannelId { get; set; }

    [MaxLength(10, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Canal Wireless")]
    public string ChannelName { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Relaciones

    public ICollection<Node>? Nodes { get; set; }
}