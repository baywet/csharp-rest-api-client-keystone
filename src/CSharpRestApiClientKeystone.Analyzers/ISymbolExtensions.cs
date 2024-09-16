using System;

using Microsoft.CodeAnalysis;

namespace CSharpRestApiClientKeystone.Analyzers;

public static class ISymbolExtensions
{
    public static bool IsExpectedType(this ISymbol symbol, string expectedSymbolName, string expectedNamespaceName = "")
    {
        return symbol is not null &&
                !string.IsNullOrEmpty(expectedSymbolName) && 
                symbol.Name.Equals(expectedSymbolName, StringComparison.Ordinal) &&
                (string.IsNullOrEmpty(expectedNamespaceName)
                    || symbol.ContainingNamespace.ToDisplayString().Equals(expectedNamespaceName, StringComparison.Ordinal));
    }
    public static bool ContainsBaseTypeInHierarchy(this ITypeSymbol typeSymbol, string expectedSymbolName, string expectedNamespaceName = "")
    {
        return typeSymbol is not null &&
                (typeSymbol.IsExpectedType(expectedSymbolName, expectedNamespaceName) ||
                    typeSymbol.BaseType is not null &&
                    typeSymbol.BaseType.ContainsBaseTypeInHierarchy(expectedSymbolName, expectedNamespaceName));
    }
}