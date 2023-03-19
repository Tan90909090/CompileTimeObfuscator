using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Reflection;
using CompileTimeObfuscator.Tests.TestUtils;
using Xunit;

namespace CompileTimeObfuscator.Tests;
public static class DeobfuscationResultTests
{
    private static readonly string[] StringTestData = new string[]
    {
        string.Empty,
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
        "寿限無寿限無五劫の擦切海砂利水魚の水行末雲来末風来末食う寝るところに住むところ藪小路のぶら小路パイポパイポパイポのシューリンガンシューリンガンのグーリンダイグーリンダイのポンポコピーのポンポコナーの長久命の長助",
        "👨‍👩‍👧‍👦",
    };
    private static readonly byte[][] BytesTestData = new byte[][]
    {
        Array.Empty<byte>(),
        new byte[]{ 0 },
        Enumerable.Range(0, 255).Select(x => (byte)x).ToArray(),
    };
    private static readonly int[] KeyLengthTestData = new int[]
    {
        1,
        16,
    };

    public static readonly TheoryData MatrixStringTestData = MatrixTheoryData.Create(StringTestData, KeyLengthTestData);
    public static readonly TheoryData MatrixBytesTestData = MatrixTheoryData.Create(BytesTestData, KeyLengthTestData);

    [Theory]
    [MemberData(nameof(MatrixStringTestData))]
    public static void DeobfuscatedStringReturnedFromMethodDirectlyShouldBeSameAsOriginalString(string value, int keyLength)
    {
        string source = $$"""
            public partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("{{value}}", KeyLength = {{keyLength}})]
                public static partial string M();
            }
            """;
        string result = (string)CompileAndInvokeStaticMethod(source, "C", "M");
        Assert.Equal(value, result);
    }

    [Theory]
    [MemberData(nameof(MatrixStringTestData))]
    public static void DeobfuscatedStringReturnedFromMethodAsIMemoryOwnerShouldBeSameAsOriginalString(string value, int keyLength)
    {
        string source = $$"""
            public partial class C
            {
                [CompileTimeObfuscator.ObfuscatedString("{{value}}", KeyLength = {{keyLength}})]
                public static partial System.Buffers.IMemoryOwner<char> M();
            }
            """;
        using var result = (IMemoryOwner<char>)CompileAndInvokeStaticMethod(source, "C", "M");
        Assert.Equal(value, result.Memory.ToString());
    }

    [Theory]
    [MemberData(nameof(MatrixBytesTestData))]
    public static void DeobfuscatedBytesReturnedFromMethodDirectlyShouldBeSameAsOriginalBytes(byte[] value, int keyLength)
    {
        string source = $$"""
            public partial class C
            {
                [CompileTimeObfuscator.ObfuscatedBytes({{Utils.ToByteArrayLiteralPresentation(value)}}, KeyLength = {{keyLength}})]
                public static partial byte[] M();
            }
            """;
        byte[] result = (byte[])CompileAndInvokeStaticMethod(source, "C", "M");
        Assert.Equal(value, result);
    }

    [Theory]
    [MemberData(nameof(MatrixBytesTestData))]
    public static void DeobfuscatedBytesReturnedFromMethodAsIMemoryOwnerShouldBeSameAsOriginalBytes(byte[] value, int keyLength)
    {
        string source = $$"""
            public partial class C
            {
                [CompileTimeObfuscator.ObfuscatedBytes({{Utils.ToByteArrayLiteralPresentation(value)}}, KeyLength = {{keyLength}})]
                public static partial System.Buffers.IMemoryOwner<byte> M();
            }
            """;
        using var result = (IMemoryOwner<byte>)CompileAndInvokeStaticMethod(source, "C", "M");
        Assert.Equal(value, result.Memory.ToArray());
    }

    private static object CompileAndInvokeStaticMethod(string source, string className, string methodName)
    {
        var result = CSharpGeneratorRunner.RunGenerator(source, verifyIfDiagnosticsReportedByGeneratorIsEmpty: true);

        using var peStream = new MemoryStream();
        var compilationResult = result.Compilation.Emit(peStream);
        Assert.True(compilationResult.Success);

        var assembly = Assembly.Load(peStream.ToArray());
        var type = assembly.GetType(className);
        var methodInfo = type!.GetMethod(methodName);
        return methodInfo!.Invoke(null, Array.Empty<object>())!;
    }
}
