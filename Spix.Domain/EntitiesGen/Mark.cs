using Spix.Domain.Entities;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class Mark
{
    [Key]
    public Guid MarkId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Marca")]
    public string MarkName { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Propiedad Virtual de Consulta
    [Display(Name = "Modelos")]
    public int ModelNumber => MarkModels == null ? 0 : MarkModels.Count;

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<MarkModel>? MarkModels { get; set; }

    //public ICollection<Node>? Nodes { get; set; }

    //public ICollection<Server>? Servers { get; set; }
}