using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Collections.Immutable;
using System.Collections.Generic;
using System;

namespace AutoRegisterInject;

[Generator]
public class Generator : IIncrementalGenerator
{
    private const string SCOPED_ATTRIBUTE_NAME = "RegisterScopedAttribute";
    private const string SINGLETON_ATTRIBUTE_NAME = "RegisterSingletonAttribute";
    private const string TRANSIENT_ATTRIBUTE_NAME = "RegisterTransientAttribute";
    private const string HOSTED_SERVICE_ATTRIBUTE_NAME = "RegisterHostedServiceAttribute";

    private static readonly Dictionary<string, AutoRegistrationType> RegistrationTypes = new()
    {
        [SCOPED_ATTRIBUTE_NAME] = AutoRegistrationType.Scoped,
        [SINGLETON_ATTRIBUTE_NAME] = AutoRegistrationType.Singleton,
        [TRANSIENT_ATTRIBUTE_NAME] = AutoRegistrationType.Transient,
        [HOSTED_SERVICE_ATTRIBUTE_NAME] = AutoRegistrationType.Hosted,
    };

    public void Initialize(IncrementalGeneratorInitializationContext initialisationContext)
    {
        initialisationContext.RegisterPostInitializationOutput((i) =>
        {
            i.AddSource("AutoRegisterInject.Attributes.g.cs",
                SourceText.From(SourceConstants.GENERATE_ATTRIBUTE_SOURCE, Encoding.UTF8));
        });

        var autoRegistered = initialisationContext
            .SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => GetAutoRegisteredClassDeclarations(ctx))
            .Where(autoRegisteredClass => autoRegisteredClass != null);

        var compilationModel = initialisationContext.CompilationProvider.Combine(autoRegistered.Collect());

        initialisationContext.RegisterSourceOutput(compilationModel, static (sourceContext, source) => Execute(source.Left, source.Right, sourceContext));
    }

    private static AutoRegisteredClass GetAutoRegisteredClassDeclarations(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeListSyntax in classDeclaration.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                var fullyQualifiedAttributeName = attributeSymbol.ContainingType.ToString();

                if (RegistrationTypes.TryGetValue(fullyQualifiedAttributeName, out var registrationType))
                {
                    var symbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(classDeclaration);

                    var interfaces = symbol.Interfaces
                        .Select(x => x.ToDisplayString())
                        .ToArray();

                    return new AutoRegisteredClass(symbol.ToDisplayString(),
                        registrationType,
                        interfaces);
                }
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<AutoRegisteredClass> classes, SourceProductionContext context)
    {
        var assemblyNameForMethod = compilation
            .AssemblyName
            .Replace(".", string.Empty)
            .Replace(" ", string.Empty)
            .Trim();

        // Group by name and type because we want to avoid any partial
        // declarations from popping up twice. Especially true if you
        // use another source generator that makes a partial class/file
        var registrations = classes
            .GroupBy(x => new { x.ClassName, x.RegistrationType })
            .Select(x => GetRegistration(x.Key.RegistrationType, x.Key.ClassName, x.SelectMany(d => d.Interfaces).ToArray()));

        string GetRegistration(AutoRegistrationType type, string className, string[] interfaces)
        {
            var hasInterfaces = interfaces.Any();

            return type switch
            {
                AutoRegistrationType.Scoped when !hasInterfaces
                    => string.Format(SourceConstants.GENERATE_SCOPED_SOURCE, className),
                AutoRegistrationType.Scoped when hasInterfaces
                    => string.Join(Environment.NewLine, interfaces.Select(d => string.Format(SourceConstants.GENERATE_SCOPED_INTERFACE_SOURCE, d, className))),

                AutoRegistrationType.Singleton when !hasInterfaces
                    => string.Format(SourceConstants.GENERATE_SINGLETON_SOURCE, className),
                AutoRegistrationType.Singleton when hasInterfaces
                    => string.Join(Environment.NewLine, interfaces.Select(d => string.Format(SourceConstants.GENERATE_SINGLETON_INTERFACE_SOURCE, d, className))),

                AutoRegistrationType.Transient when !hasInterfaces
                    => string.Format(SourceConstants.GENERATE_TRANSIENT_SOURCE, className),
                AutoRegistrationType.Transient when hasInterfaces
                    => string.Join(Environment.NewLine, interfaces.Select(d => string.Format(SourceConstants.GENERATE_TRANSIENT_INTERFACE_SOURCE, d, className))),

                AutoRegistrationType.Hosted // Hosted services do not support interfaces at this time
                    => string.Format(SourceConstants.GENERATE_HOSTED_SERVICE_SOURCE, className),

                _ => throw new NotImplementedException("Auto registration type not set up to output"),
            };
        }

        var formatted = string.Join(Environment.NewLine, registrations);
        var output = SourceConstants.GENERATE_CLASS_SOURCE.Replace("{0}", assemblyNameForMethod).Replace("{1}", formatted);
        context.AddSource("AutoRegisterInject.ServiceCollectionExtension.g.cs", SourceText.From(output, Encoding.UTF8));
    }
}