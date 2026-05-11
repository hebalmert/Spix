namespace Spix.xNetwork.PingHelper;

public class PingResult
{
    public string Host { get; set; } = string.Empty;
    public bool Success { get; set; }

    public long AverageTime { get; set; }
    public long MinTime { get; set; }
    public long MaxTime { get; set; }

    public int Sent { get; set; }
    public int Received { get; set; }
    public int Lost => Sent - Received;
    public double LossPercent => Sent == 0 ? 0 : (double)Lost / Sent * 100;

    public long Jitter { get; set; } // variación básica

    public List<long> Times { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
