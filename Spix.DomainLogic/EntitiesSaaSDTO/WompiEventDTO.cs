using System.Text.Json.Serialization;

namespace Spix.DomainLogic.EntitiesSaaSDTO;

/// <summary>
/// Evento recibido desde Wompi para confirmar el estado real de un pago.
/// </summary>
public class WompiEventDTO
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("data")]
    public WompiEventData? Data { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("signature")]
    public WompiEventSignature? Signature { get; set; }
}

public class WompiEventData
{
    [JsonPropertyName("transaction")]
    public WompiTransaction? Transaction { get; set; }
}

public class WompiTransaction
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("amount_in_cents")]
    public long AmountInCents { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("payment_method_type")]
    public string? PaymentMethodType { get; set; }
}

public class WompiEventSignature
{
    [JsonPropertyName("checksum")]
    public string? Checksum { get; set; }

    [JsonPropertyName("properties")]
    public List<string>? Properties { get; set; }
}
