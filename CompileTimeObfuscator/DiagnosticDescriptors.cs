using Microsoft.CodeAnalysis;

namespace CompileTimeObfuscator;

internal static class DiagnosticDescriptors
{
    private const string Category = "ObfuscatedStringGenerator";

    public static readonly DiagnosticDescriptor InvalidMethodSignatureForObfuscatedStringAttribute = new(
        id: "OSG0001",
        title: $"Invalid {ObfuscatedContentGenerator.ClassNameObfuscatedStringAttribution} usage",
        messageFormat: $$"""{{ObfuscatedContentGenerator.ClassNameObfuscatedStringAttribution}} method '{0}' must be partial, parameterless, non-generic, non-abstract, and return string or System.Buffers.IMemoryOwner<char>.""",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidMethodSignatureForObfuscatedBytesAttribute = new(
        id: "OSG0002",
        title: $"Invalid {ObfuscatedContentGenerator.ClassNameObfuscatedBytesAttribution} usage",
        messageFormat: $$"""{{ObfuscatedContentGenerator.ClassNameObfuscatedBytesAttribution}} method '{0}'must be partial, parameterless, non-generic, non-abstract, and return byte[] or System.Buffers.IMemoryOwner<byte>.""",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidValueParameter = new(
        id: "OSG0003",
        title: "Invalid vlue parameter",
        messageFormat: "The value parameter of the attribute on the method '{0}' must not be null.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidKeyLengthParameter = new(
        id: "OSG0004",
        title: "Invalid KeyLength parameter",
        messageFormat: "The KeySize parameter of the attribute on the method '{0}' must be greater than 0.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
