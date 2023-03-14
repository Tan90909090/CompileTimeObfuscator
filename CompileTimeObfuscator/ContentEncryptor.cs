using System;

namespace CompileTimeObfuscator;

internal static class XorEncryptor
{
    internal static void GenerateKeyAndEncryptContent(Span<byte> content, Span<byte> key)
    {
        var random = Utils.ThreadLocalRandom.Value;

        foreach (ref byte b in key)
        {
            b = (byte)random.Next(1, 255);
        }

        for (int i = 0; i < content.Length; i++)
        {
            content[i] ^= key[i % key.Length];
        }
    }

    internal static string GenerateDecryptionCode(ReadOnlySpan<byte> encryptedContent, ReadOnlySpan<byte> key, bool returnAsString)
    {
        string code = $$"""
        System.ReadOnlySpan<byte> encryptedContent = {{Utils.ConvertToSpanPresentation(encryptedContent)}};
        System.ReadOnlySpan<byte> key = {{Utils.ConvertToSpanPresentation(key)}};
        {{(returnAsString ? "using " : string.Empty)}}var buffer = new {{ObfuscatedContentGenerator.FullyQualifiedClearableBufferClassName}}(encryptedContent.Length);
        var span = buffer.Memory.Span;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = (byte)(encryptedContent[i] ^ key[i % key.Length]);
        }
        return {{(returnAsString ? "System.Text.Encoding.UTF8.GetString(buffer.Memory.Span)" : "buffer")}};
        """;
        return code;
    }
}
