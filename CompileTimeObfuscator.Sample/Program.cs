using System;
using System.Buffers;
using System.Text;
using CompileTimeObfuscator;

namespace Sample;
internal partial class Program
{
    private static string PlainText() => "This is a plain string";

    [ObfuscatedString("This is an obfuscated string")]
    private static partial string ObfuscatedText1();

    [ObfuscatedString("This is an also obfuscated string")]
    private static partial IMemoryOwner<char> ObfuscatedText2();

    [ObfuscatedBytes(new byte[] { (byte)'T', (byte)'h', (byte)'i', (byte)'s', (byte)' ', (byte)'i', (byte)'s', (byte)' ', (byte)'a', (byte)' ', (byte)'b', (byte)'y', (byte)'t', (byte)'e', (byte)'s' })]
    private static partial IMemoryOwner<byte> ObuscatedBytes();

    private static void Main()
    {
        Console.WriteLine(PlainText());
        Console.WriteLine(ObfuscatedText1());

        using var memoryOwnerChar = ObfuscatedText2();
        Console.WriteLine(memoryOwnerChar.Memory.Span.ToString());

        using var memoryOwnerByte = ObuscatedBytes();
        Console.WriteLine(Encoding.UTF8.GetString(memoryOwnerByte.Memory.Span)); // Just for an explanation
    }
}
