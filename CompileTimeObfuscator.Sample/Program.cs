using System;
using System.Buffers;
using System.Text;
using CompileTimeObfuscator;

namespace Sample;
internal partial class Program
{
    private static string PlainText() => "This is a plain string";

    [ObfuscatedString("This is an obfuscated string 1")]
    private static partial string ObfuscatedText1();

    [ObfuscatedString("This is an obfuscated string 2")]
    private static partial IMemoryOwner<char> ObfuscatedText2();

    private static ReadOnlySpan<byte> PlainBytes() => "This is a plain bytes"u8;

    [ObfuscatedBytes(new byte[] { (byte)'T', (byte)'h', (byte)'i', (byte)'s', (byte)' ', (byte)'i', (byte)'s', (byte)' ', (byte)'a', (byte)'n', (byte)' ', (byte)'o', (byte)'b', (byte)'f', (byte)'u', (byte)'s', (byte)'c', (byte)'a', (byte)'t', (byte)'e', (byte)'d', (byte)' ', (byte)'b', (byte)'y', (byte)'t', (byte)'e', (byte)'s', (byte)' ', (byte)'1', })]
    private static partial byte[] ObfuscatedBytes1();

    [ObfuscatedBytes(new byte[] { (byte)'T', (byte)'h', (byte)'i', (byte)'s', (byte)' ', (byte)'i', (byte)'s', (byte)' ', (byte)'a', (byte)'n', (byte)' ', (byte)'o', (byte)'b', (byte)'f', (byte)'u', (byte)'s', (byte)'c', (byte)'a', (byte)'t', (byte)'e', (byte)'d', (byte)' ', (byte)'b', (byte)'y', (byte)'t', (byte)'e', (byte)'s', (byte)' ', (byte)'2', })]
    private static partial IMemoryOwner<byte> ObfuscatedBytes2();

    private static void Main()
    {
        Console.WriteLine(PlainText());
        Console.WriteLine(ObfuscatedText1());
        using var memoryOwnerChar = ObfuscatedText2();
        Console.WriteLine(memoryOwnerChar.Memory.Span.ToString());

        Console.WriteLine(Encoding.UTF8.GetString(PlainBytes()));
        Console.WriteLine(Encoding.UTF8.GetString(ObfuscatedBytes1()));
        using var memoryOwnerByte = ObfuscatedBytes2();
        Console.WriteLine(Encoding.UTF8.GetString(memoryOwnerByte.Memory.Span));
    }
}
