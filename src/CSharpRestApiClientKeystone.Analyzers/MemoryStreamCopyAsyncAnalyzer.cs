using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpRestApiClientKeystone.Analyzers
{
    public class MemoryStreamCopyConstant
    {
        public const string MethodName = "CopyToAsync";
        public const string ExpectedType = "Stream";
        public const string ExpectedNamespace = "System.IO";
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemoryStreamCopyAsyncAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DJSON003";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle003), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat003), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription003), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

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
            if (!memberAccessExpr.Name.Identifier.Text.Equals(MemoryStreamCopyConstant.MethodName, StringComparison.Ordinal)) return;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccessExpr);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol ||
            methodSymbol.ReceiverType is not INamedTypeSymbol receiverTypeSymbol ||
            !receiverTypeSymbol.IsExpectedType(MemoryStreamCopyConstant.ExpectedType, MemoryStreamCopyConstant.ExpectedNamespace)) return;
            if (methodSymbol.Parameters.Length == 0) return;
            // After finding the method, we capture the first argument of hte method which is our variable
            var parameter = methodSymbol.Parameters[0];
            if (invocationExpr.ArgumentList.Arguments[0].Expression is not IdentifierNameSyntax argument) return;
            var root = context.Node.SyntaxTree.GetRoot(context.CancellationToken);
            // We find the variable declaration in the document
            var variableDeclarator = root
               .DescendantNodes()
               .OfType<VariableDeclaratorSyntax>()
               .First(x => x.Identifier.Text.Equals(argument.Identifier.Text, StringComparison.Ordinal));
            var initializer = variableDeclarator.Initializer.Value;
            // We validate if the variable is initialized with a new object creation
            if (initializer is not ObjectCreationExpressionSyntax) return;
            Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpr.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
