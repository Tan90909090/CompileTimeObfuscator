using System;
using System.Buffers;

namespace CompileTimeObfuscator;

// If I write a code using stackalloc expression such as `ReadOnlySpan<byte> a = stackalloc byte[32]`
// then process will crash with a message "The target process exited with code -2146233082 (0x80131506) while evaluating the function '<LAST_CALLED_METHOD_NAME>'."
// I don't know why but I can avoid that using MemoryPool<T>.
internal static class XorObfuscator
{
    private const string CommentAboutReadOnlySpanOptimization = "// The compiler optimize a code if `new byte[]{...}` is assigned to ReadOnlySpan<byte>. https://github.com/dotnet/roslyn/pull/24621";

    internal static string GenerateCodeForDeobfuscateString(ReadOnlySpan<char> content, int keyLength, bool clearBufferWhenDisposing, bool convertToString)
    {
        var random = Utils.ThreadLocalRandom.Value;
        using var keyBuffer = MemoryPool<byte>.Shared.Rent(keyLength);
        var keySpan = keyBuffer.Memory.Span.Slice(0, keyLength);
        using var obfuscatedBuffer = MemoryPool<byte>.Shared.Rent(content.Length * 2);
        var obfuscatedSpan = obfuscatedBuffer.Memory.Span.Slice(0, content.Length * 2);

        foreach (ref byte b in keySpan)
        {
            b = (byte)random.Next(1, 255);
        }

        for (int i = content.Length - 1; i >= 0; i--)
        {
            obfuscatedSpan[2 * i + 1] = (byte)(((content[i] >> 8) & 0xFF) ^ keySpan[(2 * i + 1) % keySpan.Length]);
            obfuscatedSpan[2 * i + 0] = (byte)(((content[i] >> 0) & 0xFF) ^ keySpan[(2 * i + 0) % keySpan.Length]);
        }

        string code = $$"""
                {{CommentAboutReadOnlySpanOptimization}}
                System.ReadOnlySpan<byte> obfuscatedValue = {{Utils.ToByteArrayLiteralPresentation(obfuscatedSpan)}};
                System.ReadOnlySpan<byte> key = {{Utils.ToByteArrayLiteralPresentation(keySpan)}};
                {{(convertToString ? "using " : string.Empty)}}var buffer = new {{ObfuscatedContentGenerator.FullyQualifiedClassNameClearableBuffer}}<char>(obfuscatedValue.Length / 2, {{Utils.ToLiteralPresentation(clearBufferWhenDisposing)}});
                var span = buffer.Memory.Span;
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    byte upper = (byte)(obfuscatedValue[2 * i + 1] ^ key[(2 * i + 1) % key.Length]);
                    byte lower = (byte)(obfuscatedValue[2 * i + 0] ^ key[(2 * i + 0) % key.Length]);
                    span[i] = (char)(upper << 8 | lower);
                }
                return {{(convertToString ? "new string(buffer.Memory.Span)" : "buffer")}};
        """;
        return code;
    }

    internal static string GenerateCodeForDeobfuscateBytes(ReadOnlySpan<byte> content, int keyLength, bool clearBufferWhenDisposing, bool convertToArray)
    {
        var random = Utils.ThreadLocalRandom.Value;
        using var keyBuffer = MemoryPool<byte>.Shared.Rent(keyLength);
        var keySpan = keyBuffer.Memory.Span.Slice(0, keyLength);
        using var obfuscatedBuffer = MemoryPool<byte>.Shared.Rent(content.Length);
        var obfuscatedSpan = obfuscatedBuffer.Memory.Span.Slice(0, content.Length);

        foreach (ref byte b in keySpan)
        {
            b = (byte)random.Next(1, 255);
        }

        for (int i = content.Length - 1; i >= 0; i--)
        {
            obfuscatedSpan[i] = (byte)(content[i] ^ keySpan[i % keySpan.Length]);
        }

        string code = $$"""
                {{CommentAboutReadOnlySpanOptimization}}
                System.ReadOnlySpan<byte> obfuscatedValue = {{Utils.ToByteArrayLiteralPresentation(obfuscatedSpan)}};
                System.ReadOnlySpan<byte> key = {{Utils.ToByteArrayLiteralPresentation(keySpan)}};
                {{(convertToArray ? "using " : string.Empty)}}var buffer = new {{ObfuscatedContentGenerator.FullyQualifiedClassNameClearableBuffer}}<byte>(obfuscatedValue.Length, {{Utils.ToLiteralPresentation(clearBufferWhenDisposing)}});
                var span = buffer.Memory.Span;
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    span[i] = (byte)(obfuscatedValue[i] ^ key[i % key.Length]);
                }
                return {{(convertToArray ? "buffer.Memory.ToArray()" : "buffer")}};
        """;
        return code;
    }
}
