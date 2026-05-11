using System.Net.Sockets;
using System.Text;

namespace Spix.xNetwork.MkHelper;

public class MikrotikControl : IMikrotikControl
{
    private TcpClient? _client;
    private Stream? _stream;

    public async Task<bool> ConnectAsync(MkConnectionInfo info)
    {
        _client = new TcpClient();
        _client.Connect(info.Host, info.Port);

        _stream = _client.GetStream();

        // Login
        await SendAsync("/login");
        await SendAsync($"=name={info.Username}");
        await SendAsync($"=password={info.Password}", true);

        var response = await ReadAsync();
        return response.Any(r => r.StartsWith("!done"));
    }

    public async Task<List<string>> SendCommandAsync(string command, params string[] args)
    {
        if (_stream == null)
            throw new InvalidOperationException("Debe conectarse primero.");

        await SendAsync(command);

        foreach (var arg in args)
            await SendAsync(arg);

        await SendAsync("", true);

        return await ReadAsync();
    }

    public void Disconnect()
    {
        _stream?.Close();
        _client?.Close();
    }

    // -----------------------------
    // Métodos privados
    // -----------------------------

    private async Task SendAsync(string text, bool endSentence = false)
    {
        var bytes = Encoding.ASCII.GetBytes(text);
        var len = EncodeLength(bytes.Length);

        await _stream!.WriteAsync(len);
        await _stream.WriteAsync(bytes);

        if (endSentence)
            await _stream.WriteAsync(new byte[] { 0 });
    }

    private async Task<List<string>> ReadAsync()
    {
        var output = new List<string>();
        string buffer = "";

        while (true)
        {
            int b = _stream!.ReadByte();
            if (b == -1) break;

            if (b == 0)
            {
                output.Add(buffer);
                if (buffer.StartsWith("!done"))
                    break;

                buffer = "";
                continue;
            }

            long count = DecodeLength(b);
            for (int i = 0; i < count; i++)
                buffer += (char)_stream.ReadByte();
        }

        return output;
    }

    private long DecodeLength(int first)
    {
        if (first < 0x80)
            return first;

        if (first < 0xC0)
            return ((first & 0x7F) << 8) + _stream!.ReadByte();

        if (first < 0xE0)
            return ((first & 0x3F) << 16) + (_stream!.ReadByte() << 8) + _stream.ReadByte();

        if (first < 0xF0)
            return ((first & 0x1F) << 24) + (_stream!.ReadByte() << 16) + (_stream.ReadByte() << 8) + _stream.ReadByte();

        byte[] buf = new byte[4];
        _stream!.Read(buf, 0, 4);
        return BitConverter.ToInt32(buf);
    }

    private byte[] EncodeLength(int len)
    {
        if (len < 0x80)
            return new byte[] { (byte)len };

        if (len < 0x4000)
            return new byte[] { (byte)((len >> 8) | 0x80), (byte)len };

        if (len < 0x200000)
            return new byte[] { (byte)((len >> 16) | 0xC0), (byte)(len >> 8), (byte)len };

        if (len < 0x10000000)
            return new byte[] { (byte)((len >> 24) | 0xE0), (byte)(len >> 16), (byte)(len >> 8), (byte)len };

        return new byte[] { 0xF0, (byte)(len >> 24), (byte)(len >> 16), (byte)(len >> 8), (byte)len };
    }
}
