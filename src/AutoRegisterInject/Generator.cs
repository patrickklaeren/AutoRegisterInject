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

    private static readonly Dictionary<string, AutoRegistrationType> RegistrationTypes = new()
    {
        [SCOPED_ATTRIBUTE_NAME] = AutoRegistrationType.Scoped,
        [SINGLETON_ATTRIBUTE_NAME] = AutoRegistrationType.Singleton,
        [TRANSIENT_ATTRIBUTE_NAME] = AutoRegistrationType.Transient,
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
        var classDeclaration = (ClassDeclarationSyntax) context.Node;

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
                    var symbol = (INamedTypeSymbol) context.SemanticModel.GetDeclaredSymbol(classDeclaration);

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

        var registrations = classes.Select(GetRegistration);

        string GetRegistration(AutoRegisteredClass classDeclaration)
        {
            var hasInterfaces = classDeclaration.Interfaces.Any();

            return classDeclaration.RegistrationType switch
            {
                AutoRegistrationType.Scoped when !hasInterfaces 
                    => string.Format(SourceConstants.GENERATE_SCOPED_SOURCE, classDeclaration.ClassName),
                AutoRegistrationType.Scoped when hasInterfaces 
                    => string.Join(Environment.NewLine, classDeclaration.Interfaces.Select(d => string.Format(SourceConstants.GENERATE_SCOPED_INTERFACE_SOURCE, d, classDeclaration.ClassName))),

                AutoRegistrationType.Singleton when !hasInterfaces 
                    => string.Format(SourceConstants.GENERATE_SINGLETON_SOURCE, classDeclaration.ClassName),
                AutoRegistrationType.Singleton when hasInterfaces 
                    => string.Join(Environment.NewLine, classDeclaration.Interfaces.Select(d => string.Format(SourceConstants.GENERATE_SINGLETON_INTERFACE_SOURCE, d, classDeclaration.ClassName))),

                AutoRegistrationType.Transient when !hasInterfaces 
                    => string.Format(SourceConstants.GENERATE_TRANSIENT_SOURCE, classDeclaration.ClassName),
                AutoRegistrationType.Transient when hasInterfaces 
                    => string.Join(Environment.NewLine, classDeclaration.Interfaces.Select(d => string.Format(SourceConstants.GENERATE_TRANSIENT_INTERFACE_SOURCE, d, classDeclaration.ClassName))),

                _ => throw new NotImplementedException("Auto registration type not set up to output"),
            };
        }

        var formatted = string.Join(Environment.NewLine, registrations);
        var output = SourceConstants.GENERATE_CLASS_SOURCE.Replace("{0}", assemblyNameForMethod).Replace("{1}",  formatted);
        context.AddSource("AutoRegisterInject.ServiceCollectionExtension.g.cs", SourceText.From(output, Encoding.UTF8));
    }
}