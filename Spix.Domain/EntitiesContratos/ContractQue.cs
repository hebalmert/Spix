using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesContratos;

public class ContractQue
{
    [Key]
    public Guid ContractQueId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Contract), ResourceType = typeof(Resource))]
    public Guid ContractClientId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Server), ResourceType = typeof(Resource))]
    public Guid ServerId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.ClientIp), ResourceType = typeof(Resource))]
    public Guid IpNetId { get; set; }

    [Required(ErrorMessageResourceName = nameof(Resource.Validation_Required), ErrorMessageResourceType = typeof(Resource))]
    [Display(Name = nameof(Resource.Plan), ResourceType = typeof(Resource))]
    public Guid PlanId { get; set; }

    [MaxLength(100)]
    [Display(Name = nameof(Resource.Server), ResourceType = typeof(Resource))]
    public string? ServerName { get; set; }

    [MaxLength(100)]
    [Display(Name = nameof(Resource.ServerIp), ResourceType = typeof(Resource))]
    public string? IpServer { get; set; }

    [MaxLength(100)]
    [Display(Name = nameof(Resource.ClientIp), ResourceType = typeof(Resource))]
    public string? IpCliente { get; set; }

    [MaxLength(100)]
    [Display(Name = nameof(Resource.Plan), ResourceType = typeof(Resource))]
    public string? PlanName { get; set; }

    [MaxLength(100)]
    [Display(Name = nameof(Resource.DownUp), ResourceType = typeof(Resource))]
    public string? TotalVelocidad { get; set; }

    [MaxLength(15, ErrorMessage = " El Campo {0} debe ser menor de {1} Caracteres")]
    [Display(Name = nameof(Resource.MikrotikId), ResourceType = typeof(Resource))]
    public string? MikrotikId { get; set; }

    public ContractClient? ContractClient { get; set; }
    public Server? Server { get; set; }
    public Plan? Plan { get; set; }
    public IpNet? IpNet { get; set; }

}