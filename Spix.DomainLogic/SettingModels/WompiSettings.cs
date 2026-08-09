namespace Spix.DomainLogic.SettingModels;

/// <summary>
/// Campos realmente utilizados para el Web Checkout y el webhook de Wompi.
/// </summary>
public class WompiSettings
{
    public string? PublicKey { get; set; }

    public string? EventsSecret { get; set; }

    public string? IntegritySecret { get; set; }

    public string? CheckoutUrl { get; set; }

    public string? RedirectUrl { get; set; }
}
