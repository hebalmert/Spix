namespace Spix.DomainLogic.SettingModels;

/// <summary>
/// Campos utilizados por la suscripción recurrente y el webhook de Mercado Pago.
/// </summary>
public class MercadoPagoSettings
{
    public string? AccessToken { get; set; }

    public string? WebhookSecret { get; set; }

    public string? CurrencyId { get; set; }

    public string? BackUrl { get; set; }

    public string? NotificationUrl { get; set; }
}
