using Microsoft.CodeAnalysis;

namespace CompileTimeObfuscator;
internal static class DiagnosticDescriptors
{
    private const string Category = "ObfuscatedStringGenerator";

    public static readonly DiagnosticDescriptor MethodReturnTypeMustBeStringOrIMemoryOwnerByte = new(
        id: "OSG0001",
        title: "Return type must be string or IMemoryOwner<byte>",
        messageFormat: "The target method's return type is not string nor System.Buffers.IMemoryOwner<byte>. The return type must be one of them.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodMustNotHaveParameters = new(
        id: "OSG0002",
        title: "Must not have parameter",
        messageFormat: "The target method has parameter(s). The method must not have parameter.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodMustNotHaveTypeParameters = new(
        id: "OSG0003",
        title: "Must not have type parameter",
        messageFormat: "The target method has type parameter(s). The method must not have type parameters.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodMustBePartial = new(
        id: "OSG0004",
        title: "Must be partial",
        messageFormat: "The target method is not have partial keyword. The method must be partial method.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
