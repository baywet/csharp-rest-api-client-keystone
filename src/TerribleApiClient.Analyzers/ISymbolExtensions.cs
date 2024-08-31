using System;

using Microsoft.CodeAnalysis;

namespace TerribleApiClient.Analyzers;

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
}