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

namespace CSharpRestApiClientKeystone.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NewtonsoftDeserializationCodeFixProvider)), Shared]
    public class NewtonsoftDeserializationCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Replace Newtonsoft.Json.JsonConvert with System.Text.Json.JsonSerializer";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(NewtonsoftDeserializationAnalyzer.DiagnosticId); }
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

            var invocationExpression = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ReplaceJsonConvertAsync(context.Document, invocationExpression, c),
                    equivalenceKey: Title),
                diagnostic);
        }
        private const string SystemTextJsonNamespace = "System.Text.Json";
        private const string JsonSerializerType = "JsonSerializer";
        private const string DeserializeMethodName = "Deserialize";

        private async Task<Document> ReplaceJsonConvertAsync(Document document, InvocationExpressionSyntax invocationExpression, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            
            // Extract the type arguments from the old invocation
            var typeArgumentList = 
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is GenericNameSyntax gNSVal ?
                gNSVal.TypeArgumentList : null;
            
            var argumentList = invocationExpression.ArgumentList;
            // Apply the type arguments to the new invocation
            var newInvocation = typeArgumentList != null
                ? SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(JsonSerializerType),
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier(DeserializeMethodName),
                            typeArgumentList)),
                    argumentList)
                : SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(JsonSerializerType),
                        SyntaxFactory.IdentifierName(DeserializeMethodName)),
                    argumentList);

            
            root = root.ReplaceNode(invocationExpression, newInvocation);

            // Ensure the using directive for System.Text.Json is present
            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(SystemTextJsonNamespace));
            if (root is CompilationUnitSyntax compilationUnit && !compilationUnit.Usings.Any(u => u.Name.ToString().Equals(SystemTextJsonNamespace, StringComparison.OrdinalIgnoreCase)))
            {
                root = compilationUnit.AddUsings(usingDirective);
            }

            return document.WithSyntaxRoot(root);
        }
    }
}
