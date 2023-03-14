using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace CompileTimeObfuscator;
internal static class Utils
{
    // I don't know it is guaranteed that generator is executed in a single thread or not.
    internal static ThreadLocal<Random> ThreadLocalRandom = new(() => new Random());

    internal static string SanitizeStringForFileName(string str)
    {
        return str
            .Replace("global::", "")
            .Replace("<", "_")
            .Replace(">", "_");
    }

    internal static string ConvertToByteArrayLiteralPresentation(byte[] bytes)
    {
        return $$"""new byte[]{{{string.Join(",", bytes.Select(b => $"0x{b:X02}"))}}}""";
    }

    internal static string ConvertToSpanPresentation(ReadOnlySpan<byte> bytes)
    {
        string prefix = "stackalloc byte[]{";
        string suffix = "}";
        var builder = new StringBuilder(capacity: bytes.Length * 4 + prefix.Length + suffix.Length);
        builder.Append(prefix);
        bool first = true;
        foreach (byte b in bytes)
        {
            if (!first)
            {
                builder.Append(",");
            }
            builder.Append(b);
            first = false;
        }
        builder.Append(suffix);
        return builder.ToString();
    }
}
