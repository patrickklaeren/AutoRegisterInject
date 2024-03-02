using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoRegisterInject;

public static class SymbolExtensions
{
    public static AttributeData GetAttribute(this ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes().First(ad => ad.AttributeClass?.Name == attributeName);
    }
    
    public static T GetParameterValues<T>(this AttributeData attributeData, string parameterName)
        where T : class
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

        return attributeData.ConstructorArguments[parameterIndex].Values as T;
    }
}