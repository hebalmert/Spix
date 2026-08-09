using System.Security.Cryptography;
using System.Text;

namespace Spix.AppInfra.Payments;

/// <summary>
/// Verifica la firma x-signature de los webhooks enviados por Mercado Pago.
/// </summary>
public static class MercadoPagoSignature
{
    public static bool Verify(string? xSignature, string? requestId, string? dataId, string secret)
    {
        if (string.IsNullOrWhiteSpace(xSignature) || string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        string? timestamp = null;
        string? signature = null;

        foreach (string part in xSignature.Split(','))
        {
            string[] values = part.Split('=', 2);
            if (values.Length != 2)
            {
                continue;
            }

            string key = values[0].Trim();
            string value = values[1].Trim();
            if (key == "ts")
            {
                timestamp = value;
            }
            else if (key == "v1")
            {
                signature = value;
            }
        }

        if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        string manifest = $"id:{(dataId ?? string.Empty).ToLowerInvariant()};request-id:{requestId};ts:{timestamp};";
        string expectedSignature = ComputeHmacSha256Hex(manifest, secret);

        return string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha256Hex(string message, string secret)
    {
        byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        using HMACSHA256 hmac = new(secretBytes);
        byte[] hashBytes = hmac.ComputeHash(messageBytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
