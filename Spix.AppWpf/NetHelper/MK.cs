using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Spix.AppWpf.NetHelper;

// Ejecuta el protocolo API de MikroTik directamente desde el equipo Windows.
public class MK
{
    private readonly Stream _connection;
    private readonly TcpClient _client;

    public MK(string ip, int port)
    {
        _client = new TcpClient();
        _client.Connect(ip, port);
        _connection = _client.GetStream();
    }

    // Libera la conexion TCP local despues de terminar una accion sobre MikroTik.
    public void Close()
    {
        _connection.Close();
        _client.Close();
    }

    // Autentica las credenciales configuradas para el servidor MikroTik.
    public bool Login(string username, string password)
    {
        Send("/login");
        Send($"=name={username}");
        Send($"=password={password}", true);

        var response = Read();
        return response.Count > 0 && response[0] == "!done";
    }

    // Envia una palabra sin cerrar la sentencia del protocolo MikroTik.
    public void Send(string command)
    {
        Send(command, false);
    }

    // Envia una palabra y opcionalmente termina la sentencia del protocolo MikroTik.
    public void Send(string command, bool endSentence)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(command.ToCharArray());
        byte[] length = EncodeLength(bytes.Length);

        _connection.Write(length, 0, length.Length);
        _connection.Write(bytes, 0, bytes.Length);

        if (endSentence)
        {
            _connection.WriteByte(0);
        }
    }

    // Lee todas las palabras devueltas por la API hasta que MikroTik cierra la sentencia.
    public List<string> Read()
    {
        var output = new List<string>();
        string currentWord = string.Empty;
        var temporary = new byte[4];

        while (true)
        {
            int readByte = _connection.ReadByte();
            if (readByte < 0)
            {
                break;
            }

            temporary[3] = (byte)readByte;

            if (temporary[3] == 0)
            {
                output.Add(currentWord);

                if (currentWord.StartsWith("!done", StringComparison.Ordinal))
                {
                    break;
                }

                currentWord = string.Empty;
                continue;
            }

            long count = DecodeLength(temporary);
            if (count < 0)
            {
                break;
            }

            for (long index = 0; index < count; index++)
            {
                int character = _connection.ReadByte();
                if (character < 0)
                {
                    return output;
                }

                currentWord += (char)character;
            }
        }

        return output;
    }

    // Conserva el calculo requerido por dispositivos MikroTik que usan el desafio legado.
    public string EncodePassword(string password, string hash)
    {
        byte[] hashBytes = new byte[hash.Length / 2];

        for (int index = 0; index <= hash.Length - 2; index += 2)
        {
            hashBytes[index / 2] = byte.Parse(
                hash.Substring(index, 2),
                System.Globalization.NumberStyles.HexNumber);
        }

        byte[] passwordBytes = new byte[1 + password.Length + hashBytes.Length];
        passwordBytes[0] = 0;
        Encoding.ASCII.GetBytes(password.ToCharArray()).CopyTo(passwordBytes, 1);
        hashBytes.CopyTo(passwordBytes, 1 + password.Length);

        using MD5 md5 = MD5.Create();
        byte[] encodedPassword = md5.ComputeHash(passwordBytes);

        return string.Concat(encodedPassword.Select(value => value.ToString("x2")));
    }

    // Convierte el encabezado binario de MikroTik en la longitud de la siguiente palabra.
    private long DecodeLength(byte[] temporary)
    {
        if (temporary[3] < 0x80)
        {
            return temporary[3];
        }

        if (temporary[3] < 0xC0)
        {
            int nextByte = _connection.ReadByte();
            return nextByte < 0
                ? -1
                : BitConverter.ToInt32(new byte[] { (byte)nextByte, temporary[3], 0, 0 }, 0) ^ 0x8000;
        }

        if (temporary[3] < 0xE0)
        {
            int middleByte = _connection.ReadByte();
            int lowByte = _connection.ReadByte();
            return middleByte < 0 || lowByte < 0
                ? -1
                : BitConverter.ToInt32(new byte[] { (byte)lowByte, (byte)middleByte, temporary[3], 0 }, 0) ^ 0xC00000;
        }

        if (temporary[3] < 0xF0)
        {
            int secondHighByte = _connection.ReadByte();
            int secondLowByte = _connection.ReadByte();
            int lowByte = _connection.ReadByte();
            return secondHighByte < 0 || secondLowByte < 0 || lowByte < 0
                ? -1
                : BitConverter.ToInt32(new byte[] { (byte)lowByte, (byte)secondLowByte, (byte)secondHighByte, temporary[3] }, 0) ^ unchecked((int)0xE0000000);
        }

        if (temporary[3] != 0xF0)
        {
            return -1;
        }

        int fourthByte = _connection.ReadByte();
        int thirdByte = _connection.ReadByte();
        int secondByte = _connection.ReadByte();
        int firstByte = _connection.ReadByte();

        if (fourthByte < 0 || thirdByte < 0 || secondByte < 0 || firstByte < 0)
        {
            return -1;
        }

        temporary[3] = (byte)fourthByte;
        temporary[2] = (byte)thirdByte;
        temporary[1] = (byte)secondByte;
        temporary[0] = (byte)firstByte;

        return BitConverter.ToInt32(temporary, 0);
    }

    // Codifica la longitud de una palabra siguiendo el protocolo API de MikroTik.
    private static byte[] EncodeLength(int length)
    {
        if (length < 0x80)
        {
            byte[] temporary = BitConverter.GetBytes(length);
            return new[] { temporary[0] };
        }

        if (length < 0x4000)
        {
            byte[] temporary = BitConverter.GetBytes(length | 0x8000);
            return new[] { temporary[1], temporary[0] };
        }

        if (length < 0x200000)
        {
            byte[] temporary = BitConverter.GetBytes(length | 0xC00000);
            return new[] { temporary[2], temporary[1], temporary[0] };
        }

        if (length < 0x10000000)
        {
            byte[] temporary = BitConverter.GetBytes(length | unchecked((int)0xE0000000));
            return new[] { temporary[3], temporary[2], temporary[1], temporary[0] };
        }

        byte[] longTemporary = BitConverter.GetBytes(length);
        return new[] { (byte)0xF0, longTemporary[3], longTemporary[2], longTemporary[1], longTemporary[0] };
    }
}
