using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TerribleApiClient.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JsonStringDeserializationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DJSON002";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

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
            //TODO check the namespace of the symbol
            if (invocationExpr.ArgumentList.Arguments is not {Count:> 0} argumentList) return;
            var firstArgument = argumentList[0].Expression;
            if (firstArgument is IdentifierNameSyntax && 
                    context.SemanticModel.GetSymbolInfo(firstArgument).Symbol is ILocalSymbol localSymbol &&
                    localSymbol.Type.Name.Equals("String", StringComparison.Ordinal) &&
                    localSymbol.Type.ContainingNamespace.Name.Equals(nameof(System), StringComparison.Ordinal))
            {
                var diagnostic = Diagnostic.Create(Rule, firstArgument.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
