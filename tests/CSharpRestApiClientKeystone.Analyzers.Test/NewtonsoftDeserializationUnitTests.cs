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
    public class NewtonsoftDeserializationUnitTests : ConventionCodeFixVerifier
    {
        [Fact]
        public Task EmptySourceAsync() => VerifyCSharpByConventionAsync();

        [Fact]
        public Task NewtonsoftInUseAsync() => VerifyCSharpByConventionAsync();
        protected override CodeFixProvider GetCSharpCodeFixProvider() => new NewtonsoftDeserializationCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new NewtonsoftDeserializationAnalyzer();
    }
}
