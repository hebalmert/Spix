using Spix.DomainLogic.ModelUtility;
using System.Net.NetworkInformation;

namespace Spix.xNetwork.PingHelper;

public class PingControl : IPingControl
{
    public async Task<ActionResponse<PingResult>> PingAsync(string host, int attempts = 4, int timeout = 4000)
    {
        var result = new PingResult { Host = host, Sent = attempts };

        try
        {
            var ping = new Ping();

            for (int i = 0; i < attempts; i++)
            {
                var reply = await ping.SendPingAsync(host, timeout);

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

            // Si todos fallaron
            if (result.Received == 0)
            {
                result.Success = false;
                result.Message = "Host no alcanzado en ningún intento";
                return new ActionResponse<PingResult>
                {
                    WasSuccess = false,
                    Message = result.Message,
                    Result = result
                };
            }

            // Filtrar tiempos válidos
            var validTimes = result.Times.Where(t => t >= 0).ToList();

            result.MinTime = validTimes.Min();
            result.MaxTime = validTimes.Max();
            result.AverageTime = (long)validTimes.Average();

            // Jitter básico = diferencia entre max y min
            result.Jitter = result.MaxTime - result.MinTime;

            result.Success = true;
            result.Message = $"Ping OK. Promedio: {result.AverageTime} ms, Perdidos: {result.Lost} ({result.LossPercent:0.0}%)";

            return new ActionResponse<PingResult>
            {
                WasSuccess = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<PingResult>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
