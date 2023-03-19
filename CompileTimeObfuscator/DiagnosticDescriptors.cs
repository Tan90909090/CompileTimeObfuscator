using Microsoft.CodeAnalysis;

namespace CompileTimeObfuscator;
/// <summary>Privodes a set of <see cref="DiagnosticDescriptor"/> used by <see cref="CompileTimeObfuscatorGenerator"/>.</summary>
public static class DiagnosticDescriptors
{
    private const string Category = "CompileTimeObfuscator";

    public static readonly DiagnosticDescriptor InvalidMethodSignatureForObfuscatedStringAttribute = new(
        id: "CTO0001",
        title: $"Invalid {CompileTimeObfuscatorGenerator.ClassNameObfuscatedStringAttribution} usage",
        messageFormat: $$"""{{CompileTimeObfuscatorGenerator.ClassNameObfuscatedStringAttribution}} method '{0}' must be partial, parameterless, non-generic, non-abstract, and return string or System.Buffers.IMemoryOwner<char>.""",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidMethodSignatureForObfuscatedBytesAttribute = new(
        id: "CTO0002",
        title: $"Invalid {CompileTimeObfuscatorGenerator.ClassNameObfuscatedBytesAttribution} usage",
        messageFormat: $$"""{{CompileTimeObfuscatorGenerator.ClassNameObfuscatedBytesAttribution}} method '{0}'must be partial, parameterless, non-generic, non-abstract, and return byte[] or System.Buffers.IMemoryOwner<byte>.""",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidValueParameter = new(
        id: "CTO0003",
        title: "Invalid vlue parameter",
        messageFormat: "The value parameter of the attribute on the method '{0}' must not be null.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidKeyLengthParameter = new(
        id: "CTO0004",
        title: "Invalid KeyLength parameter",
        messageFormat: "The KeySize parameter of the attribute on the method '{0}' must be between 1 and 65536.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
