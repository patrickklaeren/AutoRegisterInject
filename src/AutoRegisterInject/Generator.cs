using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Collections.Immutable;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AutoRegisterInject;

[Generator]
public class Generator : IIncrementalGenerator
{
    private const string SCOPED_ATTRIBUTE_NAME = "RegisterScopedAttribute";
    private const string SINGLETON_ATTRIBUTE_NAME = "RegisterSingletonAttribute";
    private const string TRANSIENT_ATTRIBUTE_NAME = "RegisterTransientAttribute";
    private const string HOSTED_SERVICE_ATTRIBUTE_NAME = "RegisterHostedServiceAttribute";
    private const string AUTO_INTERFACE_ATTRIBUTE_NAME = "AutoInterfaceAttribute";
    
    private const string KEYED_SCOPED_ATTRIBUTE_NAME = "RegisterKeyedScopedAttribute";
    private const string KEYED_SINGLETON_ATTRIBUTE_NAME = "RegisterKeyedSingletonAttribute";
    private const string KEYED_TRANSIENT_ATTRIBUTE_NAME = "RegisterKeyedTransientAttribute";
    
    private const string TRY_SCOPED_ATTRIBUTE_NAME = "TryRegisterScopedAttribute";
    private const string TRY_SINGLETON_ATTRIBUTE_NAME = "TryRegisterSingletonAttribute";
    private const string TRY_TRANSIENT_ATTRIBUTE_NAME = "TryRegisterTransientAttribute";
    
    private const string TRY_KEYED_SCOPED_ATTRIBUTE_NAME = "TryRegisterKeyedScopedAttribute";
    private const string TRY_KEYED_SINGLETON_ATTRIBUTE_NAME = "TryRegisterKeyedSingletonAttribute";
    private const string TRY_KEYED_TRANSIENT_ATTRIBUTE_NAME = "TryRegisterKeyedTransientAttribute";

    private const string ONLY_REGISTER_AS = "onlyRegisterAs";
    private const string SERVICE_KEY = "serviceKey"; 
    
    private static readonly DiagnosticDescriptor KeyedServiceDiagnostic = new(
        "ARI001",
        "Keyed registration requires a service key",
        "{0} requires a service key to be passed as an argument. Service Key argument was null, empty, or whitespace.",
        "AutoRegisterInject",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    private static readonly string[] IgnoredInterfaces =
    [
        "System.IDisposable",
        "System.IAsyncDisposable"
    ];
    
    public void Initialize(IncrementalGeneratorInitializationContext initialisationContext)
    {
        initialisationContext.RegisterPostInitializationOutput((i) =>
        {
            i.AddSource("AutoRegisterInject.Attributes.g.cs",
                SourceText.From(SourceConstants.GENERATE_ATTRIBUTE_SOURCE, Encoding.UTF8));
        });

        var autoRegisters = CombineRegistrations(
            CreateRegistrationProvider(initialisationContext, SCOPED_ATTRIBUTE_NAME, AutoRegistrationType.Scoped),
            CreateRegistrationProvider(initialisationContext, KEYED_SCOPED_ATTRIBUTE_NAME, AutoRegistrationType.KeyedScoped),
            CreateRegistrationProvider(initialisationContext, TRY_SCOPED_ATTRIBUTE_NAME, AutoRegistrationType.TryScoped),
            CreateRegistrationProvider(initialisationContext, TRY_KEYED_SCOPED_ATTRIBUTE_NAME, AutoRegistrationType.TryKeyedScoped),
            CreateRegistrationProvider(initialisationContext, SINGLETON_ATTRIBUTE_NAME, AutoRegistrationType.Singleton),
            CreateRegistrationProvider(initialisationContext, KEYED_SINGLETON_ATTRIBUTE_NAME, AutoRegistrationType.KeyedSingleton),
            CreateRegistrationProvider(initialisationContext, TRY_SINGLETON_ATTRIBUTE_NAME, AutoRegistrationType.TrySingleton),
            CreateRegistrationProvider(initialisationContext, TRY_KEYED_SINGLETON_ATTRIBUTE_NAME, AutoRegistrationType.TryKeyedSingleton),
            CreateRegistrationProvider(initialisationContext, TRANSIENT_ATTRIBUTE_NAME, AutoRegistrationType.Transient),
            CreateRegistrationProvider(initialisationContext, KEYED_TRANSIENT_ATTRIBUTE_NAME, AutoRegistrationType.KeyedTransient),
            CreateRegistrationProvider(initialisationContext, TRY_TRANSIENT_ATTRIBUTE_NAME, AutoRegistrationType.TryTransient),
            CreateRegistrationProvider(initialisationContext, TRY_KEYED_TRANSIENT_ATTRIBUTE_NAME, AutoRegistrationType.TryKeyedTransient),
            CreateRegistrationProvider(initialisationContext, HOSTED_SERVICE_ATTRIBUTE_NAME, AutoRegistrationType.Hosted));

        var autoInterfaces = initialisationContext.SyntaxProvider
            .ForAttributeWithMetadataName(
                AUTO_INTERFACE_ATTRIBUTE_NAME,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (attributeContext, _) => GetAutoInterfaces(attributeContext))
            .SelectMany(static (interfaces, _) => interfaces)
            .Collect();

        var assemblyName = initialisationContext.CompilationProvider
            .Select(static (compilation, _) => compilation.AssemblyName);

        initialisationContext.RegisterSourceOutput(
            assemblyName.Combine(autoRegisters),
            static (sourceContext, source) => Execute(source.Left, source.Right, sourceContext));

        initialisationContext.RegisterSourceOutput(autoInterfaces, static (sourceContext, interfaces) =>
        {
            foreach (var autoInterface in interfaces
                .GroupBy(static x => new { x.Namespace, x.InterfaceName })
                .Select(static x => x.OrderBy(y => y.AttributeStart).First())
                .OrderBy(static x => x.AttributeStart))
            {
                EmitAutoInterface(autoInterface, sourceContext);
            }
        });
    }

    private static IncrementalValuesProvider<AutoRegisteredClass> CreateRegistrationProvider(
        IncrementalGeneratorInitializationContext context,
        string attributeName,
        AutoRegistrationType registrationType)
    {
        return context.SyntaxProvider
            .ForAttributeWithMetadataName(
                attributeName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: (attributeContext, _) => GetAutoRegisteredClassDeclarations(attributeContext, registrationType))
            .SelectMany(static (registrations, _) => registrations);
    }

    private static IncrementalValueProvider<ImmutableArray<AutoRegisteredClass>> CombineRegistrations(
        params IncrementalValuesProvider<AutoRegisteredClass>[] providers)
    {
        var registrations = providers[0].Collect();

        foreach (var provider in providers.Skip(1))
        {
            registrations = registrations
                .Combine(provider.Collect())
                .Select(static (source, _) => source.Left.AddRange(source.Right));
        }

        return registrations;
    }

    private static ImmutableArray<AutoRegisteredClass> GetAutoRegisteredClassDeclarations(
        GeneratorAttributeSyntaxContext context,
        AutoRegistrationType registrationType)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return ImmutableArray<AutoRegisteredClass>.Empty;
        }

        var attributeData = context.Attributes[0];
        var typeName = symbol.ToDisplayString();
        var serviceKey = GetConstructorArgument(attributeData, SERVICE_KEY) ?? string.Empty;

        if (registrationType == AutoRegistrationType.Hosted)
        {
            return ImmutableArray.Create(CreateRegistration(typeName, registrationType, string.Empty, serviceKey, context));
        }

        var onlyRegisterAs = GetConstructorArrayArgument(attributeData, ONLY_REGISTER_AS);
        var interfaces = onlyRegisterAs.Length > 0
            ? symbol.AllInterfaces
                .Select(static x => x.ToDisplayString())
                .Where(interfaceName => onlyRegisterAs.Contains(interfaceName))
            : GetRegistrationInterfaces(symbol, context);

        var registrations = interfaces
            .Select(interfaceName => CreateRegistration(typeName, registrationType, interfaceName, serviceKey, context))
            .ToImmutableArray();

        return registrations.Length > 0
            ? registrations
            : ImmutableArray.Create(CreateRegistration(typeName, registrationType, string.Empty, serviceKey, context));
    }

    private static ImmutableArray<AutoInterface> GetAutoInterfaces(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol || context.TargetNode is not ClassDeclarationSyntax classDeclaration)
        {
            return ImmutableArray<AutoInterface>.Empty;
        }

        var interfaceNames = GetAutoInterfaceNames(symbol, classDeclaration);
        if (interfaceNames.Length == 0)
        {
            return ImmutableArray<AutoInterface>.Empty;
        }

        var namespaceName = symbol.ContainingNamespace?.IsGlobalNamespace is false
            ? symbol.ContainingNamespace.ToDisplayString()
            : string.Empty;
        var accessibility = symbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
        var membersSource = GetInterfaceMembersSource(symbol);
        var attributeStart = context.Attributes[0].ApplicationSyntaxReference?.Span.Start ?? context.TargetNode.SpanStart;

        return interfaceNames
            .Select(interfaceName => new AutoInterface(namespaceName, interfaceName, accessibility, membersSource, attributeStart))
            .ToImmutableArray();
    }

    private static IEnumerable<string> GetRegistrationInterfaces(INamedTypeSymbol symbol, GeneratorAttributeSyntaxContext context)
    {
        var interfaces = symbol.Interfaces
            .Select(static x => x.ToDisplayString())
            .Where(static interfaceName => !IgnoredInterfaces.Contains(interfaceName));

        if (HasAutoInterfaceAttribute(symbol) && context.TargetNode is ClassDeclarationSyntax classDeclaration)
        {
            interfaces = interfaces.Concat(GetAutoInterfaceTypeNames(symbol, classDeclaration));
        }

        return interfaces.Distinct();
    }

    private static IEnumerable<string> GetAutoInterfaceTypeNames(INamedTypeSymbol symbol, ClassDeclarationSyntax classDeclaration)
    {
        var namespaceName = symbol.ContainingNamespace?.IsGlobalNamespace is false
            ? symbol.ContainingNamespace.ToDisplayString()
            : string.Empty;

        return GetAutoInterfaceNames(symbol, classDeclaration)
            .Select(interfaceName => string.IsNullOrWhiteSpace(namespaceName)
                ? interfaceName
                : $"{namespaceName}.{interfaceName}");
    }

    private static ImmutableArray<string> GetAutoInterfaceNames(INamedTypeSymbol symbol, ClassDeclarationSyntax classDeclaration)
    {
        if (classDeclaration.BaseList is null)
        {
            return ImmutableArray<string>.Empty;
        }

        var resolvedInterfaces = symbol.AllInterfaces
            .Where(static x => x.TypeKind == TypeKind.Interface)
            .Select(static x => x.Name)
            .ToArray();

        return classDeclaration.BaseList.Types
            .Select(static baseType => GetSimpleTypeName(baseType.Type))
            .Where(static name => name is { Length: > 0 } && name[0] == 'I')
            .Where(name => !resolvedInterfaces.Contains(name))
            .Distinct()
            .ToImmutableArray();
    }

    private static string GetSimpleTypeName(TypeSyntax type)
    {
        return type switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
            AliasQualifiedNameSyntax aliasQualifiedName => aliasQualifiedName.Name.Identifier.ValueText,
            _ => null,
        };
    }

    private static string GetInterfaceMembersSource(INamedTypeSymbol symbol)
    {
        var members = symbol
            .GetMembers()
            .Select(GetInterfaceMemberSource)
            .Where(static source => !string.IsNullOrWhiteSpace(source))
            .ToArray();

        return string.Join(Environment.NewLine, members);
    }

    private static string GetInterfaceMemberSource(ISymbol member)
    {
        if (member.DeclaredAccessibility != Accessibility.Public || member.IsStatic)
        {
            return null;
        }

        return member switch
        {
            IMethodSymbol { MethodKind: MethodKind.Ordinary } method => GetMethodSource(method),
            IPropertySymbol property => GetPropertySource(property),
            IEventSymbol @event => GetEventSource(@event),
            _ => null,
        };
    }

    private static string GetMethodSource(IMethodSymbol method)
    {
        var returnType = method.ReturnType.ToDisplayString();
        var typeParameters = method.TypeParameters.Length == 0
            ? string.Empty
            : $"<{string.Join(", ", method.TypeParameters.Select(static typeParameter => typeParameter.Name))}>";
        var parameters = string.Join(", ", method.Parameters.Select(GetParameterSource));
        var constraints = GetTypeParameterConstraintsSource(method.TypeParameters);

        return $"    {returnType} {method.Name}{typeParameters}({parameters}){constraints};";
    }

    private static string GetParameterSource(IParameterSymbol parameter)
    {
        var modifiers = GetParameterModifiersSource(parameter);
        var defaultValue = GetParameterDefaultValueSource(parameter);

        return $"{modifiers}{parameter.Type.ToDisplayString()} {parameter.Name}{defaultValue}";
    }

    private static string GetParameterModifiersSource(IParameterSymbol parameter)
    {
        var paramsModifier = parameter.IsParams ? "params " : string.Empty;
        var refModifier = parameter.RefKind switch
        {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            _ => string.Empty,
        };

        return paramsModifier + refModifier;
    }

    private static string GetParameterDefaultValueSource(IParameterSymbol parameter)
    {
        var syntax = parameter.DeclaringSyntaxReferences
            .Select(static reference => reference.GetSyntax())
            .OfType<ParameterSyntax>()
            .FirstOrDefault();

        if (syntax?.Default is not null)
        {
            return $" = {syntax.Default.Value}";
        }

        if (!parameter.HasExplicitDefaultValue)
        {
            return string.Empty;
        }

        return parameter.ExplicitDefaultValue is null
            ? " = null"
            : $" = {FormatDefaultValue(parameter.ExplicitDefaultValue)}";
    }

    private static string FormatDefaultValue(object value)
    {
        return value switch
        {
            bool boolValue => boolValue ? "true" : "false",
            char charValue => $"'{charValue}'",
            string stringValue => $"\"{stringValue.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"",
            float floatValue => $"{floatValue.ToString("R", CultureInfo.InvariantCulture)}F",
            double doubleValue => $"{doubleValue.ToString("R", CultureInfo.InvariantCulture)}D",
            decimal decimalValue => $"{decimalValue.ToString(CultureInfo.InvariantCulture)}M",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    private static string GetTypeParameterConstraintsSource(ImmutableArray<ITypeParameterSymbol> typeParameters)
    {
        var constraints = typeParameters
            .Select(GetTypeParameterConstraintSource)
            .Where(static constraint => !string.IsNullOrWhiteSpace(constraint))
            .ToArray();

        return constraints.Length == 0
            ? string.Empty
            : " " + string.Join(" ", constraints);
    }

    private static string GetTypeParameterConstraintSource(ITypeParameterSymbol typeParameter)
    {
        var constraints = new List<string>();

        if (typeParameter.HasUnmanagedTypeConstraint)
        {
            constraints.Add("unmanaged");
        }
        else if (typeParameter.HasValueTypeConstraint)
        {
            constraints.Add("struct");
        }
        else if (typeParameter.HasReferenceTypeConstraint)
        {
            constraints.Add("class");
        }

        if (typeParameter.HasNotNullConstraint)
        {
            constraints.Add("notnull");
        }

        constraints.AddRange(typeParameter.ConstraintTypes.Select(static type => type.ToDisplayString()));

        if (typeParameter.HasConstructorConstraint)
        {
            constraints.Add("new()");
        }

        return constraints.Count == 0
            ? string.Empty
            : $"where {typeParameter.Name} : {string.Join(", ", constraints)}";
    }

    private static string GetPropertySource(IPropertySymbol property)
    {
        var accessors = property.SetMethod is null ? "get;" : "get; set;";
        return $"    {property.Type.ToDisplayString()} {property.Name} {{ {accessors} }}";
    }

    private static string GetEventSource(IEventSymbol @event)
    {
        return $"    event {@event.Type.ToDisplayString()} {@event.Name};";
    }

    private static bool HasAutoInterfaceAttribute(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(static attribute => attribute.AttributeClass?.Name == AUTO_INTERFACE_ATTRIBUTE_NAME);
    }

    private static AutoRegisteredClass CreateRegistration(
        string typeName,
        AutoRegistrationType registrationType,
        string interfaceName,
        string serviceKey,
        GeneratorAttributeSyntaxContext context)
    {
        var attributeStart = context.Attributes[0].ApplicationSyntaxReference?.Span.Start ?? context.TargetNode.SpanStart;

        return new AutoRegisteredClass(
            typeName,
            registrationType,
            interfaceName,
            serviceKey,
            context.TargetNode.SpanStart,
            attributeStart);
    }

    private static string GetConstructorArgument(AttributeData attributeData, string parameterName)
    {
        if (attributeData.AttributeConstructor is null)
        {
            return null;
        }

        var parameterIndex = GetParameterIndex(attributeData, parameterName);
        return parameterIndex >= 0
            ? attributeData.ConstructorArguments[parameterIndex].Value?.ToString()
            : null;
    }

    private static ImmutableArray<string> GetConstructorArrayArgument(AttributeData attributeData, string parameterName)
    {
        if (attributeData.AttributeConstructor is null)
        {
            return ImmutableArray<string>.Empty;
        }

        var parameterIndex = GetParameterIndex(attributeData, parameterName);
        if (parameterIndex < 0)
        {
            return ImmutableArray<string>.Empty;
        }

        return attributeData
            .ConstructorArguments[parameterIndex]
            .Values
            .Select(static x => x.Value?.ToString())
            .Where(static x => x is not null)
            .ToImmutableArray();
    }

    private static void Execute(string assemblyName, ImmutableArray<AutoRegisteredClass> classes, SourceProductionContext context)
    {
        var assemblyNameForMethod = assemblyName
            .Replace(".", string.Empty)
            .Replace(" ", string.Empty)
            .Trim();

        var registrations = classes
            .GroupBy(static x => new { x.ClassName, x.ClassDeclarationStart })
            .SelectMany(static x => x.Where(y => y.AttributeStart == x.Min(z => z.AttributeStart)))
            .GroupBy(static x => new { x.ClassName, x.RegistrationType, x.InterfaceName, x.ServiceKey })
            .Select(static x => x.OrderBy(y => y.AttributeStart).First())
            .OrderBy(static x => x.AttributeStart)
            .ThenBy(static x => x.InterfaceName)
            .Select(x => GetRegistration(x.RegistrationType, x.ClassName, x.InterfaceName, x.ServiceKey))
            .Where(static x => x is not null);

        var formatted = string.Join(Environment.NewLine, registrations);
        var output = SourceConstants.GENERATE_CLASS_SOURCE.Replace("{0}", assemblyNameForMethod).Replace("{1}", formatted);
        context.AddSource("AutoRegisterInject.ServiceCollectionExtension.g.cs", SourceText.From(output, Encoding.UTF8));
        return;

        string GetRegistration(AutoRegistrationType type, string className, string interfaceName, string serviceKey)
        {
            var hasInterface = !string.IsNullOrWhiteSpace(interfaceName);
            var isServiceKeyEmptyOrNull = string.IsNullOrWhiteSpace(serviceKey);

            if (IsKeyedRegistration(type) && isServiceKeyEmptyOrNull)
            {
                context.ReportDiagnostic(Diagnostic.Create(KeyedServiceDiagnostic, Location.None, type));
                return null;
            }

            return type switch
            {
                AutoRegistrationType.Scoped when !hasInterface
                    => string.Format(SourceConstants.GENERATE_SCOPED_SOURCE, className),
                AutoRegistrationType.Scoped
                    => string.Format(SourceConstants.GENERATE_SCOPED_INTERFACE_SOURCE, interfaceName, className),

                AutoRegistrationType.Singleton when !hasInterface
                    => string.Format(SourceConstants.GENERATE_SINGLETON_SOURCE, className),
                AutoRegistrationType.Singleton
                    => string.Format(SourceConstants.GENERATE_SINGLETON_INTERFACE_SOURCE, interfaceName, className),

                AutoRegistrationType.Transient when !hasInterface
                    => string.Format(SourceConstants.GENERATE_TRANSIENT_SOURCE, className),
                AutoRegistrationType.Transient
                    => string.Format(SourceConstants.GENERATE_TRANSIENT_INTERFACE_SOURCE, interfaceName, className),
                
                AutoRegistrationType.TryTransient when !hasInterface
                    => string.Format(SourceConstants.GENERATE_TRY_TRANSIENT_SOURCE, className),
                AutoRegistrationType.TryTransient
                    => string.Format(SourceConstants.GENERATE_TRY_TRANSIENT_INTERFACE_SOURCE, interfaceName, className),
                
                AutoRegistrationType.TryScoped when !hasInterface
                    => string.Format(SourceConstants.GENERATE_TRY_SCOPED_SOURCE, className),
                AutoRegistrationType.TryScoped
                    => string.Format(SourceConstants.GENERATE_TRY_SCOPED_INTERFACE_SOURCE, interfaceName, className),
                
                AutoRegistrationType.TrySingleton when !hasInterface
                    => string.Format(SourceConstants.GENERATE_TRY_SINGLETON_SOURCE, className),
                AutoRegistrationType.TrySingleton
                    => string.Format(SourceConstants.GENERATE_TRY_SINGLETON_INTERFACE_SOURCE, interfaceName, className),
                
                AutoRegistrationType.KeyedScoped when !hasInterface
                    => string.Format(SourceConstants.GENERATE_KEYED_SCOPED_SOURCE, className, serviceKey),
                AutoRegistrationType.KeyedScoped
                    => string.Format(SourceConstants.GENERATE_KEYED_SCOPED_INTERFACE_SOURCE, interfaceName, className, serviceKey),
                
                AutoRegistrationType.KeyedSingleton when !hasInterface
                    => string.Format(SourceConstants.GENERATE_KEYED_SINGLETON_SOURCE, className, serviceKey),
                AutoRegistrationType.KeyedSingleton
                    => string.Format(SourceConstants.GENERATE_KEYED_SINGLETON_INTERFACE_SOURCE, interfaceName, className, serviceKey),
                
                AutoRegistrationType.KeyedTransient when !hasInterface
                    => string.Format(SourceConstants.GENERATE_KEYED_TRANSIENT_SOURCE, className, serviceKey),
                AutoRegistrationType.KeyedTransient
                    => string.Format(SourceConstants.GENERATE_KEYED_TRANSIENT_INTERFACE_SOURCE, interfaceName, className, serviceKey),
                
                AutoRegistrationType.TryKeyedScoped when !hasInterface
                    => string.Format(SourceConstants.GENERATE_TRY_KEYED_SCOPED_SOURCE, className, serviceKey),
                AutoRegistrationType.TryKeyedScoped
                    => string.Format(SourceConstants.GENERATE_TRY_KEYED_SCOPED_INTERFACE_SOURCE, interfaceName, className, serviceKey),
                
                AutoRegistrationType.TryKeyedSingleton when !hasInterface
                    => string.Format(SourceConstants.GENERATE_TRY_KEYED_SINGLETON_SOURCE, className, serviceKey),
                AutoRegistrationType.TryKeyedSingleton
                    => string.Format(SourceConstants.GENERATE_TRY_KEYED_SINGLETON_INTERFACE_SOURCE, interfaceName, className, serviceKey),
                
                AutoRegistrationType.TryKeyedTransient when !hasInterface
                    => string.Format(SourceConstants.GENERATE_TRY_KEYED_TRANSIENT_SOURCE, className, serviceKey),
                AutoRegistrationType.TryKeyedTransient
                    => string.Format(SourceConstants.GENERATE_TRY_KEYED_TRANSIENT_INTERFACE_SOURCE, interfaceName, className, serviceKey),

                AutoRegistrationType.Hosted // Hosted services do not support interfaces at this time
                    => string.Format(SourceConstants.GENERATE_HOSTED_SERVICE_SOURCE, className),

                _ => throw new NotImplementedException("Auto registration type not set up to output"),
            };
        }
    }

    private static void EmitAutoInterface(AutoInterface autoInterface, SourceProductionContext context)
    {
        var source = string.IsNullOrWhiteSpace(autoInterface.Namespace)
            ? $@"// <auto-generated>
//     Automatically generated by AutoRegisterInject.
//     Changes made to this file may be lost and may cause undesirable behaviour.
// </auto-generated>
#nullable enable
{autoInterface.Accessibility} interface {autoInterface.InterfaceName}
{{
{autoInterface.MembersSource}
}}"
            : $@"// <auto-generated>
//     Automatically generated by AutoRegisterInject.
//     Changes made to this file may be lost and may cause undesirable behaviour.
// </auto-generated>
#nullable enable
namespace {autoInterface.Namespace}
{{

{autoInterface.Accessibility} interface {autoInterface.InterfaceName}
{{
{autoInterface.MembersSource}
}}
}}";

        context.AddSource($"AutoRegisterInject.{autoInterface.InterfaceName}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static int GetParameterIndex(AttributeData attributeData, string parameterName)
    {
        if (attributeData.AttributeConstructor is { } constructor)
        {
            for (var i = 0; i < constructor.Parameters.Length; i++)
            {
                if (constructor.Parameters[i].Name == parameterName)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static bool IsKeyedRegistration(AutoRegistrationType type)
    {
        return type is AutoRegistrationType.KeyedScoped
            or AutoRegistrationType.KeyedSingleton
            or AutoRegistrationType.KeyedTransient
            or AutoRegistrationType.TryKeyedScoped
            or AutoRegistrationType.TryKeyedSingleton
            or AutoRegistrationType.TryKeyedTransient;
    }
}
