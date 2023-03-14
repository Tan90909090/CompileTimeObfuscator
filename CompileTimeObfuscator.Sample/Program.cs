using System;
using System.Buffers;
using System.Text;
using CompileTimeObfuscator;

namespace Sample;
internal partial class Program
{
    private static string GetPlainText() => "This is a plain string";

    [ObfuscatedString("This is an obfuscated string")]
    private static partial string GetObfuscatedText();

    [ObfuscatedBytes(new byte[] { (byte)'T', (byte)'h', (byte)'i', (byte)'s', (byte)' ', (byte)'i', (byte)'s', (byte)' ', (byte)'a', (byte)' ', (byte)'b', (byte)'y', (byte)'t', (byte)'e', (byte)'s' })]
    private static partial IMemoryOwner<byte> GetObuscatedBytes();

    private static void Main()
    {
        Console.WriteLine(GetPlainText());
        Console.WriteLine(GetObfuscatedText());

        using var memoryOwner = GetObuscatedBytes();
        Console.WriteLine(Encoding.UTF8.GetString(memoryOwner.Memory.Span)); // Just for an explanation
    }
}
