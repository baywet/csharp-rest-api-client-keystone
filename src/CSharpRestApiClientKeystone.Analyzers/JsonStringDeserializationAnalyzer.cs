using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpRestApiClientKeystone.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JsonStringDeserializationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DJSON002";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle001), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat001), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription001), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not InvocationExpressionSyntax invocationExpr) return;
            if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccessExpr) return;
            if (!memberAccessExpr.Name.Identifier.Text.Equals("Deserialize", StringComparison.Ordinal)) return;
            if (context.SemanticModel.GetSymbolInfo(memberAccessExpr).Symbol is not IMethodSymbol methodSymbol ||
                methodSymbol.ReceiverType is not INamedTypeSymbol receiverTypeSymbol ||
                !receiverTypeSymbol.IsExpectedType("JsonSerializer", "System.Text.Json")) return;
            if (invocationExpr.ArgumentList.Arguments is not {Count:> 0} argumentList) return;
            var firstArgument = argumentList[0].Expression;
            if (firstArgument is IdentifierNameSyntax && 
                    context.SemanticModel.GetSymbolInfo(firstArgument).Symbol is ILocalSymbol localSymbol &&
                    localSymbol.Type.Name.Equals(nameof(String), StringComparison.Ordinal) &&
                    localSymbol.Type.ContainingNamespace.Name.Equals(nameof(System), StringComparison.Ordinal))
            {
                var diagnostic = Diagnostic.Create(Rule, firstArgument.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
