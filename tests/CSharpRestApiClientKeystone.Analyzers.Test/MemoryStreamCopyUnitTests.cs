using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;
using System.Threading.Tasks;

namespace CSharpRestApiClientKeystone.Analyzers.Test
{
    public class MemoryStreamCopyUnitTests : ConventionCodeFixVerifier
    {
        [Fact]
        public Task MemoryStreamCopyAsync() => VerifyCSharpByConventionAsync();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => null;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new MemoryStreamCopyAsyncAnalyzer();
    }
}
