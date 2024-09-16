using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpRestApiClientKeystone.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReadAsStringInUseAsyncAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DJSON002";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.DJson002Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.DJson002MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.DJson002Description), Resources.ResourceManager, typeof(Resources));
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
            if (memberAccessExpr.Name.Identifier.Text.Equals("ConfigureAwait"))
            {
                if (memberAccessExpr.Expression is not InvocationExpressionSyntax parentInvocationExpr) return;
                if (parentInvocationExpr.Expression is not MemberAccessExpressionSyntax parentMemberAccessExpr) return;
                memberAccessExpr = parentMemberAccessExpr;
            }
            if (!memberAccessExpr.Name.Identifier.Text.Equals("ReadAsStringAsync", StringComparison.Ordinal)) return;
            Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpr.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
