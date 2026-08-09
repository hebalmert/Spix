using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesSaaSDTO;

public class SubscriptionAccessDTO
{
    public bool HasAccess { get; set; }
    public bool IsTrial { get; set; }
    public int CorporationId { get; set; }
    public int? SoftPlanId { get; set; }
    public string? SoftPlanName { get; set; }
    public CorporationSubscriptionStatus? Status { get; set; }
    public DateTime? ValidUntilUtc { get; set; }
    public int DaysRemaining { get; set; }
    public string? Message { get; set; }
}
