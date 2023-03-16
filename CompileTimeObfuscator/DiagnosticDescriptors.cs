using Microsoft.CodeAnalysis;

namespace CompileTimeObfuscator;

internal static class DiagnosticDescriptors
{
    private const string Category = "ObfuscatedStringGenerator";

    public static readonly DiagnosticDescriptor InvalidObfuscatedStringAttribute = new(
        id: "OSG0001",
        title: $"Invalid {ObfuscatedContentGenerator.ObfuscatedStringAttributionClassName} usage",
        messageFormat: $"{ObfuscatedContentGenerator.ObfuscatedStringAttributionClassName} method must be partial, parameterless, non-generic, non-abstract, and return string or System.Buffers.IMemoryOwner<char>.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidObfuscatedBytesAttribute = new(
        id: "OSG0002",
        title: $"Invalid {ObfuscatedContentGenerator.ObfuscatedBytesAttributionClassName} usage",
        messageFormat: $"{ObfuscatedContentGenerator.ObfuscatedBytesAttributionClassName} method must be partial, parameterless, non-generic, non-abstract, and return byte[] or System.Buffers.IMemoryOwner<byte>.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ValueParameterIsNull = new(
        id: "OSG0003",
        title: $"Value must not be null",
        messageFormat: $"The value parameter of the attribute must not be null.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
