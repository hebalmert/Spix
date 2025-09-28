using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Spix.DomainLogic.DTOs;
using Spix.DomainLogic.ResponcesSec;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Spix.AppInfra.SmsHelper;

public class SendSms : ISendSms
{
    private readonly SendSmsSetting _settings;

    public SendSms(IOptions<SendSmsSetting> options)
    {
        _settings = options.Value;
        TwilioClient.Init(_settings.SendAccountSidKey, _settings.SendAuthTokenKey);
    }

    public async Task<string> SendAsync(string toPhoneNumber, string messageBody)
    {
        var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
        {
            Body = messageBody,
            MessagingServiceSid = _settings.SendSmsServiceSidKey
        };

        var message = await MessageResource.CreateAsync(messageOptions);
        return message.Sid;
    }

    public async Task<Dictionary<string, string>> SendBulkPersonalizedAsync(IEnumerable<SmsRecipient> recipients, string mensajeBase, int batchSize = 20, int delayMs = 500)
    {
        var results = new ConcurrentDictionary<string, string>();
        var batches = recipients.Chunk(batchSize);

        foreach (var batch in batches)
        {
            var tasks = batch.Select(async r =>
            {
                var mensajeFinal = $"Hola {r.Nombre}, {mensajeBase}";

                try
                {
                    var sid = await SendAsync(r.Telefono!, mensajeFinal);
                    results[r.Telefono!] = sid;
                }
                catch (Exception ex)
                {
                    results[r.Telefono!] = $"Error: {ex.Message}";
                }
            });

            await Task.WhenAll(tasks);
            await Task.Delay(delayMs);
        }

        return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}