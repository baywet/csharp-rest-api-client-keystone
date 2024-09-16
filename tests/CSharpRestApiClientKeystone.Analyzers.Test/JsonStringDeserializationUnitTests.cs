using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using Xunit;
using CSharpRestApiClientKeystone.Analyzers;
using System.Threading.Tasks;

namespace CSharpRestApiClientKeystone.Analyzers.Test
{
    public class JsonStringDeserializationUnitTests : ConventionCodeFixVerifier
    {
        [Fact]
        public Task EmptySourceAsync() => VerifyCSharpByConventionAsync();

        [Fact]
        public Task StringInUseAsync() => VerifyCSharpByConventionAsync();
        [Fact]
        public Task StringInUseNoVariableAsync() => VerifyCSharpByConventionAsync();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new JsonStringDeserializationCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new JsonStringDeserializationAnalyzer();
    }
}
