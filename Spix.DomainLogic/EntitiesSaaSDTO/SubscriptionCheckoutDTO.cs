namespace Spix.DomainLogic.EntitiesSaaSDTO;

public class SubscriptionCheckoutDTO
{
    public string CheckoutUrl { get; set; } = null!;
    public Guid CorporationSubscriptionId { get; set; }
}
