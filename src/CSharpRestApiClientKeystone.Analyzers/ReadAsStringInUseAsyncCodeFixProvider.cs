using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpRestApiClientKeystone.Analyzers
{
    public class SyntaxRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            // Check if the variable is of type string
            TypeSyntax variableType = node.Declaration.Type;
            if (variableType is PredefinedTypeSyntax predefinedType && StringComparer.Ordinal.Equals("string", predefinedType.Keyword.Text))
            {
                // Create a new variable declaration with 'var'
                VariableDeclarationSyntax newDeclaration = node.Declaration.WithType(SyntaxFactory.IdentifierName("var"));

                // Create a new using statement
                LocalDeclarationStatementSyntax usingStatement = SyntaxFactory.LocalDeclarationStatement(newDeclaration)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.UsingKeyword)))
                .WithTrailingTrivia(node.GetTrailingTrivia());

                // Return the new using statement
                return usingStatement;
            }

            return base.VisitLocalDeclarationStatement(node);
        }
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReadAsStringInUseAsyncCodeFixProvider)), Shared]
    public class ReadAsStringInUseAsyncCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Replace string value with Stream";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ReadAsStringInUseAsyncAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            Diagnostic diagnostic = context.Diagnostics[0];
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            MemberAccessExpressionSyntax memberExpression = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                .OfType<MemberAccessExpressionSyntax>()
                .First(x => x.Name.Identifier.Text.Equals("ReadAsStringAsync", StringComparison.Ordinal));

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ApplyCodeFix(context.Document, memberExpression, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> ApplyCodeFix(Document document, MemberAccessExpressionSyntax memberExpression, CancellationToken cancellationToken)
        {
            Document newDocument = await ReplaceStringIdentifierWithStreamAsync(document, memberExpression, cancellationToken);
            newDocument = await ReplaceStringToUsingVarStatement(newDocument, memberExpression, cancellationToken);
            return newDocument;
        }

        private async Task<Document> ReplaceStringToUsingVarStatement(Document document, MemberAccessExpressionSyntax memberExpression, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxRewriter rewriter = new SyntaxRewriter();
            SyntaxNode newRoot = rewriter.Visit(root);
            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> ReplaceStringIdentifierWithStreamAsync(Document document, MemberAccessExpressionSyntax memberExpression, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            MemberAccessExpressionSyntax newMemberAccess = memberExpression.WithName(SyntaxFactory.IdentifierName("ReadAsStreamAsync"));
            SyntaxNode newRoot = root.ReplaceNode(memberExpression, newMemberAccess);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
