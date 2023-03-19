using CompileTimeObfuscator.Tests.TestUtils;
using Xunit;

namespace CompileTimeObfuscator.Tests;
// Xunit does not test methods belonging to interfaces and Xunit throws an exception when test methods belonging to structs.
// So I decide to write test methods belonging to a class with dynamic source generation.
public static partial class BelongingTypeTests
{
    [Fact]
    public static void GeneratorShouldGenerateSourceIfMethodBelongToClass()
    {
        string source = """
            public partial class Class
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
    public static void GeneratorShouldGenerateSourceIfMethodBelongToStruct()
    {
        string source = """
            public partial struct Struct
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
    public static void GeneratorShouldGenerateSourceIfMethodBelongToRecordClassWithClassKeyword()
    {
        string source = """
            public partial record class RecordClassWithClassKeyword
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
    public static void GeneratorShouldGenerateSourceIfMethodBelongToRecordClassWithoutClassKeyword()
    {
        string source = """
            public partial record RecordClassWithoutClassKeyword
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
    public static void GeneratorShouldGenerateSourceIfMethodBelongToRecordStruct()
    {
        string source = """
            public partial record struct RecordStruct
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
    public static void GeneratorShouldGenerateSourceIfMethodBelongToInterface()
    {
        string source = """
            public partial interface Interface
            {
                [CompileTimeObfuscator.ObfuscatedString("test")]
                private static partial string M();
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.NotNull(result.GeneratedSource);
        Assert.Empty(result.DiagnosticsReportedByGenerator);
    }

    [Theory]
    [InlineData("<T>")]
    [InlineData("<T1,T2>")]
    public static void GeneratorShouldGenerateSourceIfMethodBelongToGenericClass(string genericParameter)
    {
        string source = $$"""
            public partial class Class{{genericParameter}}
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
    public static void GeneratorShouldGenerateSourceIfMethodBelongToVeryComplexHierarchy()
    {
        string source = """
            namespace N1
            {
                namespace N2
                {
                    readonly ref partial struct A<T>
                    {
                        readonly partial record struct B<U>
                        {
                            abstract partial class C<V>
                            {
                                static partial class D<W>
                                {
                                    sealed partial record class E<X>
                                    {
                                        partial interface F<Y>
                                        {
                                            [CompileTimeObfuscator.ObfuscatedString("test")]
                                            private static partial string M();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            """;
        var result = CSharpGeneratorRunner.RunGenerator(source);
        Assert.NotNull(result.GeneratedSource);
        Assert.Empty(result.DiagnosticsReportedByGenerator);
    }
}
