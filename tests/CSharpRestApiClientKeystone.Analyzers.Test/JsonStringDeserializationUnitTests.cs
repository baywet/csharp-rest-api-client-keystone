using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;
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
