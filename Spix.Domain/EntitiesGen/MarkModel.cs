using Spix.Domain.Entities;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesGen;

public class MarkModel
{
    [Key]
    public Guid MarkModelId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Modelo")]
    public string MarkModelName { get; set; } = null!;

    public Guid MarkId { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    [NotMapped]
    [Display(Name = "Marca")]
    public string? MarkName { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Mark? Mark { get; set; }

    //public ICollection<Node>? Nodes { get; set; }

    //public ICollection<Server>? Servers { get; set; }
}