using SendGrid;
using SendGrid.Helpers.Mail;
using Spix.DomainLogic.EntitiesEmailDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.xNotification.Interfaces;
using System.Net;
using System.Net.Mail;
using Response = Spix.DomainLogic.ModelUtility.Response;

namespace Spix.xNotification.Implements;

public class EmailDeliveryService : IEmailDeliveryService
{
    public async Task<Response> SendAsync(EmailDeliveryDTO model)
    {
        if (model.ProviderType == EmailProviderType.SendGrid)
        {
            return await SendWithSendGridAsync(model.SendGridApiKey!, model.FromEmail, model.FromName,
                model.To, model.NameTo, model.Subject, model.Body);
        }

        return await SendWithSmtpAsync(model.SmtpHost!, model.SmtpPort, model.SmtpUseSsl,
            model.SmtpUser!, model.SmtpPassword!, model.FromEmail, model.FromName,
            model.To, model.NameTo, model.Subject, model.Body);
    }

    public async Task<Response> SendWithSendGridAsync(string apiKey, string fromEmail, string fromName,
        string to, string nameTo, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(fromEmail) ||
            string.IsNullOrWhiteSpace(to))
        {
            return new Response { IsSuccess = false, Message = "SendGrid settings are incomplete." };
        }

        if (!IsValidEmail(fromEmail))
        {
            return new Response { IsSuccess = false, Message = $"El correo remitente no es valido: {fromEmail}" };
        }

        if (!IsValidEmail(to))
        {
            return new Response { IsSuccess = false, Message = $"El correo destino no es valido: {to}" };
        }

        try
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var toEmail = new EmailAddress(to, nameTo);
            var singleEmail = MailHelper.CreateSingleEmail(from, toEmail, subject, "Spix Email", body);

            var answer = await client.SendEmailAsync(singleEmail);
            if (answer.IsSuccessStatusCode)
            {
                return new Response { IsSuccess = true };
            }

            string error = await answer.Body.ReadAsStringAsync();
            return new Response { IsSuccess = false, Message = $"SendGrid error: {answer.StatusCode} {error}" };
        }
        catch (Exception ex)
        {
            return new Response { IsSuccess = false, Message = $"SendGrid exception: {ex.Message}" };
        }
    }

    public async Task<Response> SendWithSmtpAsync(string smtpHost, int smtpPort, bool smtpUseSsl,
        string smtpUser, string smtpPassword, string fromEmail, string fromName,
        string to, string nameTo, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(smtpHost) ||
            string.IsNullOrWhiteSpace(smtpUser) ||
            string.IsNullOrWhiteSpace(smtpPassword) ||
            string.IsNullOrWhiteSpace(fromEmail) ||
            string.IsNullOrWhiteSpace(to))
        {
            return new Response { IsSuccess = false, Message = "SMTP settings are incomplete." };
        }

        if (!IsValidEmail(fromEmail))
        {
            return new Response { IsSuccess = false, Message = $"El correo remitente no es valido: {fromEmail}" };
        }

        if (!IsValidEmail(to))
        {
            return new Response { IsSuccess = false, Message = $"El correo destino no es valido: {to}" };
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(to, nameTo));

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = smtpUseSsl,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            await smtp.SendMailAsync(message);
            return new Response { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new Response { IsSuccess = false, Message = $"SMTP exception: {ex.Message}" };
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
