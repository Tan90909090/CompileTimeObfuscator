using System;

namespace CompileTimeObfuscator;

internal static class XorEncryptor
{
    internal static int GenerateKeyAndEncryptContent(ReadOnlySpan<char> content, Span<byte> key, Span<byte> result)
    {
        if (result.Length < content.Length * 2) { return 0; }

        var random = Utils.ThreadLocalRandom.Value;

        foreach (ref byte b in key)
        {
            b = (byte)random.Next(1, 255);
        }

        for (int i = content.Length - 1; i >= 0; i--)
        {
            result[2 * i + 1] = (byte)(((content[i] >> 8) & 0xFF) ^ key[(2 * i + 1) % key.Length]);
            result[2 * i + 0] = (byte)(((content[i] >> 0) & 0xFF) ^ key[(2 * i + 0) % key.Length]);
        }

        return content.Length * 2;
    }
    internal static int GenerateKeyAndEncryptContent(ReadOnlySpan<byte> content, Span<byte> key, Span<byte> result)
    {
        if (result.Length < content.Length) { return 0; }

        var random = Utils.ThreadLocalRandom.Value;

        foreach (ref byte b in key)
        {
            b = (byte)random.Next(1, 255);
        }

        for (int i = content.Length - 1; i >= 0; i--)
        {
            result[i] = (byte)(content[i] ^ key[i % key.Length]);
        }

        return content.Length;
    }

    internal static string GenerateDecryptionCodeForString(ReadOnlySpan<byte> encryptedContent, ReadOnlySpan<byte> key, bool convertToString)
    {
        string code = $$"""
        System.ReadOnlySpan<byte> encryptedContent = {{Utils.ConvertToSpanPresentation(encryptedContent)}};
        System.ReadOnlySpan<byte> key = {{Utils.ConvertToSpanPresentation(key)}};
        {{(convertToString ? "using " : string.Empty)}}var buffer = new {{ObfuscatedContentGenerator.FullyQualifiedClearableBufferClassName}}<char>(encryptedContent.Length / 2);
        var span = buffer.Memory.Span;
        for (int i = span.Length - 1; i >= 0; i--)
        {
            byte upper = (byte)(encryptedContent[2 * i + 1] ^ key[(2 * i + 1) % key.Length]);
            byte lower = (byte)(encryptedContent[2 * i + 0] ^ key[(2 * i + 0) % key.Length]);
            span[i] = (char)(upper << 8 | lower);
        }
        return {{(convertToString ? "new string(buffer.Memory.Span)" : "buffer")}};
        """;
        return code;
    }

    internal static string GenerateDecryptionCodeForBytes(ReadOnlySpan<byte> encryptedContent, ReadOnlySpan<byte> key)
    {
        string code = $$"""
        System.ReadOnlySpan<byte> encryptedContent = {{Utils.ConvertToSpanPresentation(encryptedContent)}};
        System.ReadOnlySpan<byte> key = {{Utils.ConvertToSpanPresentation(key)}};
        var buffer = new {{ObfuscatedContentGenerator.FullyQualifiedClearableBufferClassName}}<byte>(encryptedContent.Length);
        var span = buffer.Memory.Span;
        for (int i = span.Length - 1; i >= 0; i--)
        {
            span[i] = (byte)(encryptedContent[i] ^ key[i % key.Length]);
        }
        return buffer;
        """;
        return code;
    }
}
