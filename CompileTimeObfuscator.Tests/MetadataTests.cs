using System;
using System.IO;
using System.Text;
using CompileTimeObfuscator.Tests.TestUtils;
using Xunit;

namespace CompileTimeObfuscator.Tests;
public class MetadataTests
{
    private const string TestStringMustNotExistInMetadata = "This string must not exist in a metadata.";

    [Fact]
    public static void AttribtueArgumentStringShouldNotExistInMetadata()
    {
        string value = TestStringMustNotExistInMetadata;
        string source = $$"""
            public partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("{{value}}")]
                public static partial string M();
            }
            """;

        // Check with the two encodings to be sure. .NET 6 seems to use utf-8 encoding.
        CompileAndAssertThatValueDoesNotExistInMetadata(source, Encoding.UTF8.GetBytes(value));
        CompileAndAssertThatValueDoesNotExistInMetadata(source, Encoding.Unicode.GetBytes(value));
    }

    [Fact]
    public static void AttribtueArgumentBytesShouldNotExistInMetadata()
    {
        byte[] value = Encoding.UTF8.GetBytes(TestStringMustNotExistInMetadata);
        string source = $$"""
            public partial class C
            {
                [CompileTimeObfuscator.ObfuscatedBytes({{Utils.ToByteArrayLiteralPresentation(value)}})]
                public static partial byte[] M();
            }
            """;
        CompileAndAssertThatValueDoesNotExistInMetadata(source, value);
    }

    private static void CompileAndAssertThatValueDoesNotExistInMetadata(string source, ReadOnlySpan<byte> valueInPlainText)
    {
        var result = CSharpGeneratorRunner.RunGenerator(source, verifyIfDiagnosticsReportedByGeneratorIsEmpty: true);

        using var peStream = new MemoryStream();
        var compilationResult = result.Compilation.Emit(peStream);
        Assert.True(compilationResult.Success);

        Assert.Equal(-1, peStream.ToArray().AsSpan().IndexOf(valueInPlainText));
    }
}
