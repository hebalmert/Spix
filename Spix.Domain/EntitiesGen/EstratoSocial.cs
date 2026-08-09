using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class EstratoSocial
{
    [Key]
    public Guid EstratoSocialId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.EstratoSocial), ResourceType = typeof(Resource))]
    public string EstratoSocialName { get; set; } = null!;

    [Display(Name = nameof(Resource.ApplyTax), ResourceType = typeof(Resource))]
    public bool ApplyTax { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ICollection<ContractClient>? ContractClients { get; set; }
}
