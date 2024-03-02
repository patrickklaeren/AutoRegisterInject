using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoRegisterInject;

public static class Extensions
{
    public static AttributeData GetFirstAutoRegisterAttribute(this ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes().First(ad => ad.AttributeClass?.Name == attributeName);
    }
    
    public static string[] GetIgnoredTypeNames(this AttributeData attributeData, string parameterName)
    {
        if (attributeData.AttributeConstructor is null)
        {
            return null;
        }

        var parameterIndex = attributeData
            .AttributeConstructor
            .Parameters
            .ToList()
            .FindIndex(c => c.Name == parameterName);

        if (parameterIndex < 0)
        {
            return null;
        }

        var values = attributeData
            .ConstructorArguments[parameterIndex]
            .Values
            .Select(x => x.Value.ToString())
            .ToArray();

        return values;
    }
}