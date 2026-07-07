using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesEmails;

public class EmailProviderSetting
{
    [Key]
    public Guid EmailProviderSettingId { get; set; }

    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = null!;

    public EmailProviderType ProviderType { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string FromEmail { get; set; } = null!;

    [Required]
    [MaxLength(120)]
    public string FromName { get; set; } = null!;

    [MaxLength(2048)]
    public string? SendGridApiKeyEncrypted { get; set; }

    [MaxLength(256)]
    public string? SmtpHost { get; set; }

    public int? SmtpPort { get; set; }

    public bool SmtpUseSsl { get; set; }

    [MaxLength(256)]
    public string? SmtpUser { get; set; }

    [MaxLength(2048)]
    public string? SmtpPasswordEncrypted { get; set; }

    public bool Active { get; set; }

    public bool IsDefault { get; set; }

    public DateTime DateCreated { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public int CorporationId { get; set; }

    [NotMapped]
    public string? SendGridApiKey { get; set; }

    [NotMapped]
    public string? SmtpPassword { get; set; }

    public Corporation? Corporation { get; set; }
}
