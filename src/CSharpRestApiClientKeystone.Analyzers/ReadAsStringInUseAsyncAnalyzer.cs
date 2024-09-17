using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpRestApiClientKeystone.Analyzers
{
    public class ReadAsStringConstant {
        public const string MethodName = "ReadAsStringAsync";
        public const string ExpectedType = "HttpContent";
        public const string ExpectedNamespace = "System.Net.Http";
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReadAsStringInUseAsyncAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DJSON002";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle002), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat002), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription002), Resources.ResourceManager, typeof(Resources));
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
            if (!memberAccessExpr.Name.Identifier.Text.Equals(ReadAsStringConstant.MethodName, StringComparison.Ordinal)) return;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccessExpr);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol ||
            methodSymbol.ReceiverType is not INamedTypeSymbol receiverTypeSymbol ||
            !receiverTypeSymbol.IsExpectedType(ReadAsStringConstant.ExpectedType, ReadAsStringConstant.ExpectedNamespace)) return;
            Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpr.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
