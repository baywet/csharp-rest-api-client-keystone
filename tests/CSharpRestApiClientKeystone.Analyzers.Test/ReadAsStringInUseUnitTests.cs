using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;
using System.Threading.Tasks;

namespace CSharpRestApiClientKeystone.Analyzers.Test
{
    public class ReadAsStringInUseUnitTests : ConventionCodeFixVerifier
    {
        [Fact]
        public Task StringInUseResponseAsync() => VerifyCSharpByConventionAsync();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ReadAsStringInUseAsyncCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ReadAsStringInUseAsyncAnalyzer();
    }
}
