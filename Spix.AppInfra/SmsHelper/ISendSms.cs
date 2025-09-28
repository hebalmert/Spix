using Spix.DomainLogic.DTOs;

namespace Spix.AppInfra.SmsHelper
{
    public interface ISendSms
    {
        Task<string> SendAsync(string toPhoneNumber, string messageBody);

        Task<Dictionary<string, string>> SendBulkPersonalizedAsync(IEnumerable<SmsRecipient> recipients, string mensajeBase, int batchSize = 20, int delayMs = 500);
    }
}