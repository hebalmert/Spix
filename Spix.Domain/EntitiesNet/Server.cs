using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesNet;

public class Server
{
    [Key]
    public Guid ServerId { get; set; }

    [Display(Name = nameof(Resource.Server), ResourceType = typeof(Resource))]
    [MaxLength(50, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public string ServerName { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.IpNetwork), ResourceType = typeof(Resource))]
    public Guid IpNetworkId { get; set; }

    [Display(Name = nameof(Resource.User), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public string Usuario { get; set; } = null!;

    [Display(Name = nameof(Resource.Password), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    public string Clave { get; set; } = null!;

    [Display(Name = nameof(Resource.WanName), ResourceType = typeof(Resource))]
    [MaxLength(25, ErrorMessageResourceName = nameof(Resource.Validation_MaxLength), ErrorMessageResourceType = typeof(Resource))]
    public string WanName { get; set; } = null!;

    [Display(Name = nameof(Resource.ApiPort), ResourceType = typeof(Resource))]
    public int ApiPort { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Mark), ResourceType = typeof(Resource))]
    public Guid MarkId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Model), ResourceType = typeof(Resource))]
    public Guid MarkModelId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Zone), ResourceType = typeof(Resource))]
    public Guid ZoneId { get; set; }

    [Display(Name = nameof(Resource.Active), ResourceType = typeof(Resource))]
    public bool Active { get; set; }

    [NotMapped]
    public int StateId { get; set; }

    [NotMapped]
    public int CityId { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
    public IpNetwork? IpNetwork { get; set; }
    public Mark? Mark { get; set; }
    public MarkModel? MarkModel { get; set; }
    public Zone? Zone { get; set; }

    public ICollection<ContractServer>? ContractServers { get; set; }
    public ICollection<ContractQue>? ContractQues { get; set; }

}