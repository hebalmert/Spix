using Spix.Domain.Entities;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesGen;

public class MarkModel
{
    [Key]
    public Guid MarkModelId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.MarkModels), ResourceType = typeof(Resource))]
    public string MarkModelName { get; set; } = null!;

    [Display(Name = nameof(Resource.Mark), ResourceType = typeof(Resource))]
    public Guid MarkId { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [NotMapped]
    [Display(Name = nameof(Resource.Mark), ResourceType = typeof(Resource))]
    public string? MarkName { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Mark? Mark { get; set; }

    //public ICollection<Node>? Nodes { get; set; }

    //public ICollection<Server>? Servers { get; set; }
}