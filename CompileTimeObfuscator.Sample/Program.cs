using CompileTimeObfuscator;

namespace Sample;
internal partial class Program
{
    private static string GetPlainText() => "This is a plain string.";

    [ObfuscatedString("This is an obfuscated string.")]
    private static partial string GetObfuscatedText();

    private static void Main()
    {
        Console.WriteLine(GetPlainText());
        Console.WriteLine(GetObfuscatedText());
    }
}
