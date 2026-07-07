using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.EntitiesEmailDTO;

namespace Spix.xNotification.Interfaces;

public interface IEmailDeliveryService
{
    Task<Response> SendAsync(EmailDeliveryDTO model);

    Task<Response> SendWithSendGridAsync(string apiKey, string fromEmail, string fromName,
        string to, string nameTo, string subject, string body);

    Task<Response> SendWithSmtpAsync(string smtpHost, int smtpPort, bool smtpUseSsl,
        string smtpUser, string smtpPassword, string fromEmail, string fromName,
        string to, string nameTo, string subject, string body);
}
