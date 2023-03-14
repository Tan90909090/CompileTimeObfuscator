using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
        {{(returnAsString ? "using " : string.Empty)}}var buffer = new {{ObfuscatedContentGenerator.NameSpaceName}}.ClearableBuffer(encryptedContent.Length);
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

internal static class AesEncryptor
{
    // .net standard 2.0 does not have rich APIs for Span<T>...
    internal static (byte[] Bytes, byte[] Key, byte[] IV) EncryptContent(string content)
    {
        byte[] contentToEncrypt = Encoding.Unicode.GetBytes(content);
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateKey();
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream(capacity: content.Length + aes.BlockSize);
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(contentToEncrypt, 0, contentToEncrypt.Length);

        return (memoryStream.ToArray(), aes.Key, aes.IV);
    }

    internal static string GenerateDecryptionCode(
        byte[] encryptedContent,
        byte[] key,
        byte[] iv)
    {
        string code = $"""
        var encryptedContent = {Utils.ConvertToByteArrayLiteralPresentation(encryptedContent)};
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Mode = System.Security.Cryptography.CipherMode.CBC;
        aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
        aes.Key = {Utils.ConvertToByteArrayLiteralPresentation(key)};
        aes.IV = {Utils.ConvertToByteArrayLiteralPresentation(iv)};

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var memoryStream = new System.IO.MemoryStream(encryptedContent);
        using var cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read);
        using var decryptedStream = new System.IO.MemoryStream(capacity: encryptedContent.Length);
        cryptoStream.CopyTo(decryptedStream);

        return System.Text.Encoding.Unicode.GetString(decryptedStream.ToArray());
        """;

        return code;
    }
}
