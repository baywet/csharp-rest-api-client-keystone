using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using Xunit;
using TerribleApiClient.Analyzers;
using System.Threading.Tasks;

namespace TerribleApiClient.Analyzers.Test
{
    public class JsonStringDeserializationUnitTests : ConventionCodeFixVerifier
    {
        [Fact]
        public Task EmptySourceAsync() => VerifyCSharpByConventionAsync();

        [Fact]
        public Task StringInUseAsync() => VerifyCSharpByConventionAsync();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new JsonStringDeserializationCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new JsonStringDeserializationAnalyzer();
    }
}
