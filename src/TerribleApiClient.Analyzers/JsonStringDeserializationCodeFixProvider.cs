using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;

namespace TerribleApiClient.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JsonStringDeserializationCodeFixProvider)), Shared]
    public class JsonStringDeserializationCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Replace string value with Stream";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(JsonStringDeserializationAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var literalExpression = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<LiteralExpressionSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => ReplaceStringLiteralAsync(context.Document, literalExpression, c),
                equivalenceKey: Title),
            diagnostic);
    }

    private async Task<Document> ReplaceStringLiteralAsync(Document document, LiteralExpressionSyntax literalExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var generator = SyntaxGenerator.GetGenerator(document);

        var methodDeclaration = literalExpression.Ancestors().OfType<MethodDeclarationSyntax>().First();
        var streamVariable = methodDeclaration.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .FirstOrDefault(v => semanticModel.GetTypeInfo(v.Initializer.Value).Type.Name.Equals("Stream", StringComparison.Ordinal));
        if (streamVariable != null)
        {
            var streamIdentifier = generator.IdentifierName(streamVariable.Identifier.Text);
            var newRoot = root.ReplaceNode(literalExpression, streamIdentifier);
            return document.WithSyntaxRoot(newRoot);
        }

        return document;
    }
    }
}
