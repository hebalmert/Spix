using Spix.DomainLogic.DTOs;

namespace Spix.AppInfra.SmsHelper;

public interface IExcelParser
{
    List<SmsRecipient> ParseRecipientsFromBase64(string base64);
}