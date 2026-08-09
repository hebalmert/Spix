namespace Spix.DomainLogic.EntitiesSaaSDTO;

/// <summary>
/// Configuracion administrable de Mercado Pago. Las credenciales privadas son de solo escritura:
/// el servidor nunca las devuelve al navegador, solo informa si estan configuradas.
/// </summary>
public class MercadoPagoPlatformSettingDTO
{
    public string? Name { get; set; }

    public string? PublicKey { get; set; }

    public string? AccessToken { get; set; }

    public string? WebhookSecret { get; set; }

    public string? WebhookUrl { get; set; }

    public bool Active { get; set; }

    public bool HasPublicKey { get; set; }

    public bool HasAccessToken { get; set; }

    public bool HasWebhookSecret { get; set; }
}
