﻿using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CompileTimeObfuscator;
/// <summary>A generator class.</summary>
[Generator(LanguageNames.CSharp)]
public partial class ObfuscatedContentGenerator : IIncrementalGenerator
{
    public const string CommentAutoGenerated = "// <auto-generated/>";
    public const string NameSpaceName = "CompileTimeObfuscator";
    public const string ClassNameObfuscatedStringAttribution = "ObfuscatedStringAttribute";
    public const string FullyQualifiedClassNameObfuscatedStringAttribute = $"{NameSpaceName}.{ClassNameObfuscatedStringAttribution}";
    public const string ClassNameObfuscatedBytesAttribution = "ObfuscatedBytesAttribute";
    public const string FullyQualifiedClassNameObfuscatedBytesAttribute = $"{NameSpaceName}.{ClassNameObfuscatedBytesAttribution}";
    public const string ClassNameClearableBuffer = "ClearableBuffer";
    public const string FullyQualifiedClassNameClearableBuffer = $"{NameSpaceName}.{ClassNameClearableBuffer}";

    public const string PropertyNameKeyLength = "KeyLength";
    public const string DefaultValueKeyLength = "16";
    public const int MinKeyLength = 1;
    public const int MaxKeyLength = 65536;
    public const string PropertyNameClearBufferWhenDisposing = "ClearBufferWhenDisposing";
    public const string DefaultValueClearBufferWhenDisposing = "true";

    private const string PostInitializationOutputCode = $$"""
        {{CommentAutoGenerated}}
        #nullable enable

        using System;
        using System.Buffers;
        using System.Diagnostics;

        namespace {{NameSpaceName}};

        /// <summary>Obfuscate the specified string to preventing the string from appearing in a metadata. The obfuscated string is deobfuscated at runtime. The method must return <see cref="string"/> or <see cref="IMemoryOwner{T}"/> of type <see cref="char"/>.</summary>
        [Conditional("COMPILE_TIME_ONLY")]
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        internal sealed class {{ClassNameObfuscatedStringAttribution}}: Attribute
        {
            /// <summary>Initializes a new instance of the <see cref="{{ClassNameObfuscatedStringAttribution}}"/> with the specified string.</summary>
            /// <param name="value">The string to obfuscate.</param>
            internal {{ClassNameObfuscatedStringAttribution}}(string value)
            {
            }

            /// <summary>Indicates the key length to obfuscate. A default value is {{DefaultValueKeyLength}}.</summary>
            public int {{PropertyNameKeyLength}} = {{DefaultValueKeyLength}};

            /// <summary>Indicates whether a deobfuscated buffer will cleared after disposing an <see cref="IMemoryOwner{T}"/> object. A default value is {{DefaultValueClearBufferWhenDisposing}}.</summary>
            public bool {{PropertyNameClearBufferWhenDisposing}} = {{DefaultValueClearBufferWhenDisposing}};
        }

        /// <summary>Obfuscate the specified bytes to preventing the bytes from appearing in a metadata. The obfuscated bytes is deobfuscated at runtime. The method must return <see cref="byte"/>[] or <see cref="IMemoryOwner{T}"/> of type <see cref="byte"/>.</summary>
        [Conditional("COMPILE_TIME_ONLY")]
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        internal sealed class {{ClassNameObfuscatedBytesAttribution}}: Attribute
        {
            /// <summary>Initializes a new instance of the <see cref="{{ClassNameObfuscatedBytesAttribution}}"/> with the specified bytes.</summary>
            /// <param name="value">The bytes to obfuscate.</param>
            internal {{ClassNameObfuscatedBytesAttribution}}(byte[] value)
            {
            }

            /// <summary>Indicates the key length to obfuscate. A default value is {{DefaultValueKeyLength}}.</summary>
            public int {{PropertyNameKeyLength}} = {{DefaultValueKeyLength}};

            /// <summary>Indicates whether a deobfuscated buffer will cleared after disposing an <see cref="IMemoryOwner{T}"/> object. A default value is {{DefaultValueClearBufferWhenDisposing}}.</summary>
            public bool {{PropertyNameClearBufferWhenDisposing}} = {{DefaultValueClearBufferWhenDisposing}};
        }

        internal sealed class {{ClassNameClearableBuffer}}<T> : IMemoryOwner<T>
        {
            private T[]? _array;
            private readonly int _length;
            private readonly bool _clearBufferWhenDisposing;

            internal {{ClassNameClearableBuffer}}(int length, bool clearBufferWhenDisposing)
            {
                _array = ArrayPool<T>.Shared.Rent(length);
                _length = length;
                _clearBufferWhenDisposing = clearBufferWhenDisposing;
            }

            public void Dispose()
            {
                if (_array is null) { return; }

                ArrayPool<T>.Shared.Return(_array, _clearBufferWhenDisposing);
                _array = null;
            }

            /// <summary>Returns <see cref="Memory{T}"/> that length is the originally required length. This behavior is different from an <see cref="IMemoryOwner{T}"/> returned from <see cref="MemoryPool{T}.Shared"/>.</summary>
            public Memory<T> Memory
            {
                get
                {
                    if (_array is null) { throw new ObjectDisposedException(GetType().FullName); }

                    return new Memory<T>(_array, 0, _length);
                }
            }
        }

        """;

    private enum ObfuscationType
    {
        String,
        IMemoryOwnerChar,
        ByteArray,
        IMemoryOwnerByte,
    }

    private readonly record struct AttributeArgument(TypedConstant TypedConstantForValueParameter, int KeyLength, bool ClearBufferWhenDisposing);

    public void Initialize(IncrementalGeneratorInitializationContext initializationContext)
    {
        initializationContext.RegisterPostInitializationOutput(static context =>
            {
                context.AddSource($"{NameSpaceName}.g.cs", PostInitializationOutputCode);
            });

        void RegisterEmitterForSpecifiedAttribute(
            string fullyQualifiedAttributeName,
            ImmutableArray<ObfuscationType> validReturnTypes,
            DiagnosticDescriptor diagnosticDescriptorForInvalidMethodSignature)
        {
            var source = initializationContext.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedAttributeName,
                static (syntaxNode, cancellationToken) => true,
                static (syntaxContext, cancellationToken) => syntaxContext);
            initializationContext.RegisterSourceOutput(
                source,
                (context, source) =>
                {
                    EmitCore(context,
                        source,
                        fullyQualifiedAttributeName,
                        validReturnTypes,
                        diagnosticDescriptorForInvalidMethodSignature);
                });
        }
        RegisterEmitterForSpecifiedAttribute(
            FullyQualifiedClassNameObfuscatedStringAttribute,
            ImmutableArray.Create(ObfuscationType.String, ObfuscationType.IMemoryOwnerChar),
            DiagnosticDescriptors.InvalidMethodSignatureForObfuscatedStringAttribute);
        RegisterEmitterForSpecifiedAttribute(
            FullyQualifiedClassNameObfuscatedBytesAttribute,
            ImmutableArray.Create(ObfuscationType.ByteArray, ObfuscationType.IMemoryOwnerByte),
            DiagnosticDescriptors.InvalidMethodSignatureForObfuscatedBytesAttribute);
    }

    private static void EmitCore(
        SourceProductionContext context,
        GeneratorAttributeSyntaxContext source,
        string fullyQualifiedAttributeName,
        ImmutableArray<ObfuscationType> validReturnTypes,
        DiagnosticDescriptor diagnosticDescriptorForInvalidMethodSignature)
    {
        var methodSymbol = (IMethodSymbol)source.TargetSymbol;
        var methodDeclarationSyntax = (MethodDeclarationSyntax)source.TargetNode;

        // ContainingType property is null when the method is not contained with in a type. I'm not sure this happen when using C# language.
        var containingClassSymbol = methodSymbol.ContainingType ??
            throw new InvalidOperationException($"IMethodSymbol.ContainingType is null");

        if (GetObfuscationTypeOrReportDiagnostic(
            context,
            methodSymbol,
            methodDeclarationSyntax,
            source.SemanticModel,
            validReturnTypes,
            diagnosticDescriptorForInvalidMethodSignature) is not { } obfuscationType)
        {
            return;
        }

        if (GetAttributeArgumentOrReportDiagnostic(
            context,
            methodSymbol,
            methodDeclarationSyntax,
            fullyQualifiedAttributeName) is not { } attributeArgument)
        {
            return;
        }

        var (openingDefinitionCode, codeForClosingDefinition) = Utils.GenerateOpeningClosingTypeDefinitionCode(methodSymbol);
        string methodModifier = $"{SyntaxFacts.GetText(methodSymbol.DeclaredAccessibility)}{(methodSymbol.IsStatic ? " static" : string.Empty)} partial";
        string methodBodyCode = GenerateMethodBodyCode(attributeArgument, fullyQualifiedAttributeName, obfuscationType);
        string generatedSourceCode = $$"""
            {{CommentAutoGenerated}}
            #nullable enable

            {{openingDefinitionCode}}
                {{methodModifier}} {{methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} {{methodSymbol.Name}}()
            {{methodBodyCode}}
            {{codeForClosingDefinition}}

            """;

        string classPart = containingClassSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "__global__";
        string returnTypePart = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string generatingSourceFileName = Utils.SanitizeForFileName($"{classPart}.{methodSymbol.Name}.{returnTypePart}.g.cs");
        context.AddSource(generatingSourceFileName, generatedSourceCode);
    }

    /// <summary>Returns a return-type of the method if there is no signature error. Returns null and reports diagnostic otherwise.</summary>
    private static ObfuscationType? GetObfuscationTypeOrReportDiagnostic(
        SourceProductionContext context,
        IMethodSymbol methodSymbol,
        MethodDeclarationSyntax methodDeclarationSyntax,
        SemanticModel semanticModel,
        ImmutableArray<ObfuscationType> validReturnTypes,
        DiagnosticDescriptor diagnosticDescriptorForInvalidMethodSignature)
    {
        INamedTypeSymbol GetTypeSymbolFromName(string fullyQualifiedmetadataName) =>
            semanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedmetadataName) ??
            throw new InvalidOperationException($"{fullyQualifiedmetadataName} is not found");

        var typeIMemoryOwnerOpened = GetTypeSymbolFromName("System.Buffers.IMemoryOwner`1");
        var typeByte = GetTypeSymbolFromName("System.Byte");
        var typeChar = GetTypeSymbolFromName("System.Char");
        var typeString = GetTypeSymbolFromName("System.String");
        var typeIMemoryOwnerByte = typeIMemoryOwnerOpened.Construct(typeByte);
        var typeIMemoryOwnerChar = typeIMemoryOwnerOpened.Construct(typeChar);
        var typeByteArray = semanticModel.Compilation.CreateArrayTypeSymbol(typeByte);

        bool HasReturnType(ISymbol symbol) => SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, symbol);

        var obfuscationType = HasReturnType(typeString) ? ObfuscationType.String :
            HasReturnType(typeIMemoryOwnerChar) ? ObfuscationType.IMemoryOwnerChar :
            HasReturnType(typeByteArray) ? ObfuscationType.ByteArray :
            HasReturnType(typeIMemoryOwnerByte) ? ObfuscationType.IMemoryOwnerByte :
            (ObfuscationType?)null;

        bool validSignature = obfuscationType is not null &&
            validReturnTypes.Contains(obfuscationType.Value) &&
            methodSymbol.Parameters.Length == 0 &&
            methodSymbol.Arity == 0 &&
            methodSymbol.IsPartialDefinition &&
            !methodSymbol.IsAbstract;
        if (!validSignature)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    diagnosticDescriptorForInvalidMethodSignature,
                    methodDeclarationSyntax.Identifier.GetLocation(),
                    methodSymbol.Name));
            return null;
        }

        return obfuscationType;
    }

    /// <summary>Returns arguments for the attribute if successful. Returns null and reports diagnostic otherwise.</summary>
    private static AttributeArgument? GetAttributeArgumentOrReportDiagnostic(
        SourceProductionContext context,
        IMethodSymbol methodSymbol,
        MethodDeclarationSyntax methodDeclarationSyntax,
        string fullyQualifiedAttributeName)
    {
        var attributeData = methodSymbol.GetAttributes().Single(
            x => x.AttributeClass?.ToDisplayString() == fullyQualifiedAttributeName);

        if (attributeData.ConstructorArguments.Length != 1)
        {
            throw new InvalidOperationException($"ConstructorArguments.Length is {attributeData.ConstructorArguments.Length}");
        }

        var typedConstantForValueParameter = attributeData.ConstructorArguments[0];
        if (typedConstantForValueParameter.IsNull)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidValueParameter,
                    methodDeclarationSyntax.Identifier.GetLocation(),
                    methodSymbol.Name));
            return null;
        }

        // AttributeData.NamedArguments contains explicitly specified fileds only so if a field is not specified then we must handle as absent values.
        T GetNamedArgumentOrDefault<T>(string name, T defaultValue)
        {
            // ImmutbaleArray<T> does not have a Find method...
            foreach (var namedArgument in attributeData.NamedArguments)
            {
                if (StringComparer.Ordinal.Equals(namedArgument.Key, name))
                {
                    return (T)namedArgument.Value.Value!;
                }
            }
            return defaultValue;
        }

        int keyLength = GetNamedArgumentOrDefault(PropertyNameKeyLength, int.Parse(DefaultValueKeyLength));
        bool clearBufferWhenDisposing = GetNamedArgumentOrDefault(PropertyNameClearBufferWhenDisposing, bool.Parse(DefaultValueClearBufferWhenDisposing));

        if (keyLength is < MinKeyLength or > MaxKeyLength)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidKeyLengthParameter,
                    methodDeclarationSyntax.Identifier.GetLocation(),
                    methodSymbol.Name));
            return null;
        }

        return new AttributeArgument(typedConstantForValueParameter, keyLength, clearBufferWhenDisposing);
    }

    /// <summary>Gets a code string to deobfuscate content.</summary>
    private static string GenerateMethodBodyCode(
        in AttributeArgument attributeArgument,
        string fullyQualifiedAttributeName,
        ObfuscationType obfuscationType)
    {
        if (fullyQualifiedAttributeName == FullyQualifiedClassNameObfuscatedStringAttribute)
        {
            string str = (string)attributeArgument.TypedConstantForValueParameter.Value!;
            return XorObfuscator.GenerateMethodBodyCodeForDeobfuscateString(
                str.AsSpan(),
                attributeArgument.KeyLength,
                attributeArgument.ClearBufferWhenDisposing,
                obfuscationType == ObfuscationType.String);
        }
        else if (fullyQualifiedAttributeName == FullyQualifiedClassNameObfuscatedBytesAttribute)
        {
            byte[] bytes = attributeArgument.TypedConstantForValueParameter.Values.Select(x => (byte)x.Value!).ToArray();
            return XorObfuscator.GenerateMethodBodyCodeForDeobfuscateBytes(
                bytes,
                attributeArgument.KeyLength,
                attributeArgument.ClearBufferWhenDisposing,
                obfuscationType == ObfuscationType.ByteArray);
        }
        else
        {
            throw new ArgumentException($"Unknown value {fullyQualifiedAttributeName}", nameof(fullyQualifiedAttributeName));
        }
    }
}
