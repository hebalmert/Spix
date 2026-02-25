using Spix.Domain.Entities;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesGen;

public class Tax
{
    [Key]
    public Guid TaxId { get; set; }

    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Tax), ResourceType = typeof(Resource))]
    public string TaxName { get; set; } = null!;

    [Range(0, 99, ErrorMessage = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = nameof(Resource.Rate), ResourceType = typeof(Resource))]
    public decimal Rate { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    //Relaciones
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<Product>? Products { get; set; }

    public ICollection<ServiceClient>? ServiceClients { get; set; }

    public ICollection<Plan>? Plans { get; set; }
}