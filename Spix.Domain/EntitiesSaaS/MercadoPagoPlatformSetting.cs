using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesSaaS;

public class MercadoPagoPlatformSetting
{
    [Key]
    public Guid MercadoPagoPlatformSettingId { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = "Mercado Pago Colombia";

    [Required]
    [MaxLength(2048)]
    public string PublicKeyEncrypted { get; set; } = null!;

    [Required]
    [MaxLength(2048)]
    public string AccessTokenEncrypted { get; set; } = null!;

    [MaxLength(2048)]
    public string? WebhookSecretEncrypted { get; set; }

    [MaxLength(512)]
    public string? WebhookUrl { get; set; }

    public bool Active { get; set; }

    public DateTime DateModifiedUtc { get; set; }

    [MaxLength(256)]
    public string? UserModifiedByName { get; set; }

    [NotMapped]
    public string? PublicKey { get; set; }

    [NotMapped]
    public string? AccessToken { get; set; }

    [NotMapped]
    public string? WebhookSecret { get; set; }
}
