using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using Xunit;
using TerribleApiClient.Analyzers;

namespace TerribleApiClient.Analyzers.Test
{
    public class JsonStringDeserializationUnitTests : ConventionCodeFixVerifier
    {
        [Fact]
        public void EmptySource() => VerifyCSharpByConvention();

        [Fact]
        public void LowercaseLetters() => VerifyCSharpByConvention();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new JsonStringDeserializationCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new JsonStringDeserializationAnalyzer();
    }
}
