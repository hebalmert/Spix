using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesSaaSDTO;

public class SubscriptionCheckoutRequestDTO
{
    public int SoftPlanId { get; set; }
    public SubscriptionCycle Cycle { get; set; }
}
