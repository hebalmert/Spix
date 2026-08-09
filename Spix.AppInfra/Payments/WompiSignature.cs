using Spix.DomainLogic.EntitiesSaaSDTO;
using System.Security.Cryptography;
using System.Text;

namespace Spix.AppInfra.Payments;

/// <summary>
/// Genera la firma de integridad del checkout y valida los eventos de Wompi.
/// </summary>
public static class WompiSignature
{
    public static string BuildIntegritySignature(
        string reference,
        long amountInCents,
        string currency,
        string integritySecret)
    {
        return ToHexSha256($"{reference}{amountInCents}{currency}{integritySecret}");
    }

    public static bool VerifyEventSignature(WompiEventDTO eventDto, string eventsSecret)
    {
        if (eventDto.Signature?.Properties == null ||
            string.IsNullOrWhiteSpace(eventDto.Signature.Checksum) ||
            eventDto.Data?.Transaction == null)
        {
            return false;
        }

        var builder = new StringBuilder();

        foreach (string property in eventDto.Signature.Properties)
        {
            string? value = ResolveProperty(property, eventDto.Data.Transaction);

            if (value == null)
            {
                return false;
            }

            builder.Append(value);
        }

        builder.Append(eventDto.Timestamp);
        builder.Append(eventsSecret);

        string checksum = ToHexSha256(builder.ToString());

        return string.Equals(
            checksum,
            eventDto.Signature.Checksum,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string? ResolveProperty(string property, WompiTransaction transaction)
    {
        return property switch
        {
            "transaction.id" => transaction.Id,
            "transaction.status" => transaction.Status,
            "transaction.amount_in_cents" => transaction.AmountInCents.ToString(),
            _ => null
        };
    }

    private static string ToHexSha256(string value)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        var builder = new StringBuilder(bytes.Length * 2);

        foreach (byte valueByte in bytes)
        {
            builder.Append(valueByte.ToString("x2"));
        }

        return builder.ToString();
    }
}
