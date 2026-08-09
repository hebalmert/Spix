using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesSaaS;

public class CorporationSubscription
{
    [Key]
    public Guid CorporationSubscriptionId { get; set; }

    public int CorporationId { get; set; }

    public int SoftPlanId { get; set; }

    public SubscriptionCycle Cycle { get; set; }

    public CorporationSubscriptionStatus Status { get; set; }

    public DateTime DateCreatedUtc { get; set; }

    public DateTime? TrialStartsUtc { get; set; }

    public DateTime? TrialEndsUtc { get; set; }

    public DateTime? CurrentPeriodStartsUtc { get; set; }

    public DateTime? CurrentPeriodEndsUtc { get; set; }

    [MaxLength(180)]
    public string ExternalReference { get; set; } = null!;

    [MaxLength(120)]
    public string? MercadoPagoPreapprovalId { get; set; }

    [MaxLength(1024)]
    public string? CheckoutUrl { get; set; }

    [MaxLength(256)]
    public string? UserModifiedByName { get; set; }

    public Corporation? Corporation { get; set; }

    public SoftPlan? SoftPlan { get; set; }
}
