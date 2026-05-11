namespace Spix.xNetwork.MkHelper;

public interface IMikrotikControl
{
    Task<bool> ConnectAsync(MkConnectionInfo info);
    Task<List<string>> SendCommandAsync(string command, params string[] args);
    void Disconnect();
}
