using Spix.Core.EntitiesOper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class DocumentType
{
    [Key]
    public Guid DocumentTypeId { get; set; }

    [MaxLength(10, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Documento")]
    public string DocumentName { get; set; } = null!;

    [MaxLength(100, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = "Descripcion")]
    public string? Descripcion { get; set; }

    [Display(Name = "Activo")]
    public bool Active { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<Client>? Clients { get; set; }

    public ICollection<Contractor>? Contractors { get; set; }

    public ICollection<Supplier>? Suppliers { get; set; }
}