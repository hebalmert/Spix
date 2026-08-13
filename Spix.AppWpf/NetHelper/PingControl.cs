using Spix.DomainLogic.ModelUtility;
using System.Net.NetworkInformation;

namespace Spix.AppWpf.NetHelper;

// Ejecuta ping desde Windows para probar la red LAN o Wi-Fi del usuario.
public class PingControl : IPingControl
{
    public async Task<ActionResponse<PingResult>> PingAsync(
        string host,
        int attempts = 4,
        int timeout = 4000)
    {
        var result = new PingResult
        {
            Host = host,
            Sent = attempts
        };

        if (string.IsNullOrWhiteSpace(host))
        {
            return new ActionResponse<PingResult>
            {
                WasSuccess = false,
                Message = "Debes indicar un host o direccion IP para ejecutar el ping.",
                Result = result
            };
        }

        if (attempts <= 0 || timeout <= 0)
        {
            return new ActionResponse<PingResult>
            {
                WasSuccess = false,
                Message = "Los intentos y el tiempo de espera deben ser mayores que cero.",
                Result = result
            };
        }

        try
        {
            using var ping = new Ping();

            for (int index = 0; index < attempts; index++)
            {
                PingReply reply = await ping.SendPingAsync(host, timeout);

                if (reply.Status == IPStatus.Success)
                {
                    result.Times.Add(reply.RoundtripTime);
                    result.Received++;
                }
                else
                {
                    result.Times.Add(-1);
                }
            }

            if (result.Received == 0)
            {
                result.Success = false;
                result.Message = "Host no alcanzado en ningun intento";

                return new ActionResponse<PingResult>
                {
                    WasSuccess = false,
                    Message = result.Message,
                    Result = result
                };
            }

            List<long> validTimes = result.Times.Where(time => time >= 0).ToList();
            result.MinTime = validTimes.Min();
            result.MaxTime = validTimes.Max();
            result.AverageTime = (long)validTimes.Average();
            result.Jitter = result.MaxTime - result.MinTime;
            result.Success = true;
            result.Message = $"Ping OK. Promedio: {result.AverageTime} ms, Perdidos: {result.Lost} ({result.LossPercent:0.0}%)";

            return new ActionResponse<PingResult>
            {
                WasSuccess = true,
                Result = result
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<PingResult>
            {
                WasSuccess = false,
                Message = exception.Message,
                Result = result
            };
        }
    }
}
