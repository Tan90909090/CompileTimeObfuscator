using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CompileTimeObfuscator.Tests.TestUtils;
using Xunit;

namespace CompileTimeObfuscator.Tests;
public static partial class AttributeParameterTests
{
    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("12")]
    public static void GeneratedObfuscatedStringLengthShouldBeTwiceLengthOfOriginalString(string value)
    {
        string source = $$"""
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("{{value}}")]
                private partial string M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Empty(result.DiagnosticsReportedByGenerator);

        string obfuscatedValueLine = result.GeneratedSource!.Split("\n")
            .Single(line => line.Contains("ReadOnlySpan<byte> obfuscatedValue = "));
        string elements = Regex.Match(obfuscatedValueLine, """\{([^}]*)\}""").Groups[1].Value;
        int elementCount = elements.Length == 0 ? 0 : elements.Count(c => c == ',') + 1;
        Assert.Equal(value.Length * 2, elementCount);
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(new byte[] { 1 })]
    [InlineData(new byte[] { 1, 2 })]
    public static void GeneratedObfuscatedBytesLengthShouldBeSameAsOriginalBytes(byte[] value)
    {
        string source = $$"""
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedBytes({{Utils.ToByteArrayLiteralPresentation(value)}})]
                private partial byte[] M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Empty(result.DiagnosticsReportedByGenerator);

        string obfuscatedValueLine = result.GeneratedSource!.Split("\n")
            .Single(line => line.Contains("ReadOnlySpan<byte> obfuscatedValue = "));
        string elements = Regex.Match(obfuscatedValueLine, """\{([^}]*)\}""").Groups[1].Value;
        int elementCount = elements.Length == 0 ? 0 : elements.Count(c => c == ',') + 1;
        Assert.Equal(value.Length, elementCount);
    }

    [Fact]
    public static void GeneratorShouldReportDiagnosticIfValueIsNull()
    {
        string source = """
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString(null)]
                private partial string M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Null(result.GeneratedSource);
        Assert.Single(result.DiagnosticsReportedByGenerator);
        Assert.Equal("CTO0003", result.DiagnosticsReportedByGenerator[0].Id);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(65536)]
    public static void GeneratorShouldGenerateSourceIfKeyLengthIsBetween1And65536(int keyLength)
    {
        string source = $$"""
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("test", KeyLength = {{keyLength.ToString(CultureInfo.InvariantCulture)}})]
                private partial string M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Empty(result.DiagnosticsReportedByGenerator);

        string keyLine = result.GeneratedSource!.Split("\n")
            .Single(line => line.Contains("ReadOnlySpan<byte> key = "));
        string elements = Regex.Match(keyLine, """\{([^}]+)\}""").Groups[1].Value;
        int elementCount = elements.Count(c => c == ',') + 1;
        Assert.Equal(keyLength, elementCount);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(65537)]
    [InlineData(int.MaxValue)]
    public static void GeneratorShouldReportDiagnosticIfKeyLengthIsNotBetween1And65536(int keyLength)
    {
        string source = $$"""
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("test", KeyLength = {{keyLength.ToString(CultureInfo.InvariantCulture)}})]
                private partial string M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Null(result.GeneratedSource);
        Assert.Single(result.DiagnosticsReportedByGenerator);
        Assert.Equal("CTO0004", result.DiagnosticsReportedByGenerator[0].Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public static void GeneratorShouldGenerateSourceUsingClearBufferWhenDisposing(bool clearBufferWhenDisposing)
    {
        string source = $$"""
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("test", ClearBufferWhenDisposing = {{Utils.ToLiteralPresentation(clearBufferWhenDisposing)}})]
                private partial string M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Empty(result.DiagnosticsReportedByGenerator);

        string bufferInitializationLine = result.GeneratedSource!.Split("\n")
            .Single(line => line.Contains(CompileTimeObfuscatorGenerator.FullyQualifiedClassNameClearableBuffer));
        string value = Regex.Match(bufferInitializationLine, """([\w]*)\);""").Groups[1].Value;
        Assert.Equal(clearBufferWhenDisposing, bool.Parse(value));
    }
}
