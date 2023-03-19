using CompileTimeObfuscator.Tests.TestUtils;
using Xunit;

namespace CompileTimeObfuscator.Tests;
public static partial class MethodModifierTests
{
    [Fact]
    public static void GeneratorShouldGenerateSourceIfMethodIsNotStatic()
    {
        string source = """
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("test")]
                private partial string M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.NotNull(result.GeneratedSource);
        Assert.Empty(result.DiagnosticsReportedByGenerator);
    }

    [Fact]
    public static void GeneratorShouldGenerateSourceIfMethodIsStatic()
    {
        string source = """
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("test")]
                private static partial string M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.NotNull(result.GeneratedSource);
        Assert.Empty(result.DiagnosticsReportedByGenerator);
    }

    [Fact]
    public static void GeneratorShouldReportDiagnosticIfMethodIsNotPartial()
    {
        string source = """
            class C
            {
                [CompileTimeObfuscator.ObfuscatedString("test")]
                string M() => "";
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Null(result.GeneratedSource);
        Assert.Single(result.DiagnosticsReportedByGenerator);
        Assert.Equal("CTO0001", result.DiagnosticsReportedByGenerator[0].Id);
    }

    [Fact]
    public static void GeneratorShouldReportDiagnosticIfMethodHasParameter()
    {
        string source = """
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("test")]
                private partial string M(int x);
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Null(result.GeneratedSource);
        Assert.Single(result.DiagnosticsReportedByGenerator);
        Assert.Equal("CTO0001", result.DiagnosticsReportedByGenerator[0].Id);
    }

    [Fact]
    public static void GeneratorShouldReportDiagnosticIfMethodHasTypeParameter()
    {
        string source = """
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("test")]
                private partial string M<T>();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Null(result.GeneratedSource);
        Assert.Single(result.DiagnosticsReportedByGenerator);
        Assert.Equal("CTO0001", result.DiagnosticsReportedByGenerator[0].Id);
    }

    [Fact]
    public static void GeneratorShouldReportDiagnosticIfMethodReturnTypeIsInvalidForObfuscatedString()
    {
        string source = """
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("test")]
                private partial int M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Null(result.GeneratedSource);
        Assert.Single(result.DiagnosticsReportedByGenerator);
        Assert.Equal("CTO0001", result.DiagnosticsReportedByGenerator[0].Id);
    }

    [Fact]
    public static void GeneratorShouldReportDiagnosticIfMethodReturnTypeIsInvalidForObfuscatedBytes()
    {
        string source = """
            partial class C
            {
                [CompileTimeObfuscator.ObfuscatedBytes(new byte[]{1})]
                private partial int M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.Null(result.GeneratedSource);
        Assert.Single(result.DiagnosticsReportedByGenerator);
        Assert.Equal("CTO0002", result.DiagnosticsReportedByGenerator[0].Id);
    }
}
