using System.Text;

namespace RedirectServer.util;

public static class DecodeXorBase64
{
    public static string Decode(string input, string key)
    {
        var data = Convert.FromBase64String(input);
        if (data.Length % 2 != 0) throw new ArgumentException("Invalid encrypted data length");

        var sb = new StringBuilder(data.Length / 2);
        var keyLen = key.Length;
        for (int i = 0, kIndex = 0; i < data.Length; i += 2, kIndex++)
        {
            // SQL CAST(@x AS BINARY(2)) produced big-endian bytes  -- reconstruct ushort
            var x = (ushort)((data[i] << 8) | data[i + 1]);
            ushort kcp = key[keyLen == 0 ? 0 : (kIndex % keyLen)]; // key char code
            var p = (ushort)(x ^ kcp); // original Unicode code point
            sb.Append((char)p);
        }

        return sb.ToString();
    }
}