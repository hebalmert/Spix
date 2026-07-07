using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesEmailDTO;

public class EmailDeliveryDTO
{
    public EmailProviderType ProviderType { get; set; }

    public bool UseSendGrid => ProviderType == EmailProviderType.SendGrid;

    public string? SendGridApiKey { get; set; }

    public string? SmtpHost { get; set; }

    public int SmtpPort { get; set; }

    public bool SmtpUseSsl { get; set; }

    public string? SmtpUser { get; set; }

    public string? SmtpPassword { get; set; }

    public string FromEmail { get; set; } = null!;

    public string FromName { get; set; } = null!;

    public string To { get; set; } = null!;

    public string NameTo { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;
}
