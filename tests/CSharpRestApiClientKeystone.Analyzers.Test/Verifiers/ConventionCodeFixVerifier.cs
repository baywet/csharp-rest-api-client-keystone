using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using Xunit;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace TestHelper
{
    public abstract class ConventionCodeFixVerifier : CodeFixVerifier
    {
        public ConventionCodeFixVerifier()
        {
            var t = GetType();
            DataSourcePath = Path.Combine("../../../DataSource", t.Name);
        }

        private string DataSourcePath { get; }

        protected async Task VerifyCSharpByConventionAsync([CallerMemberName]string testName = null, CancellationToken cancellationToken = default)
        {
            await VerifyCSharpByConventionV2Async(testName, cancellationToken);
        }

        private async Task VerifyCSharpByConventionV2Async(string testName, CancellationToken cancellationToken)
        {
            var sources = await ReadSourcesAsync(testName, cancellationToken);
            var expectedResults = await ReadDiagnosticResultsFromFolderAsync(testName, cancellationToken);
            var expectedSources = await ReadExpectedSourcesAsync(testName, cancellationToken);

            await VerifyCSharpAsync(sources, expectedResults.ToArray(), expectedSources.ToArray());
        }

        private async Task<IEnumerable<DiagnosticResult>> ReadDiagnosticResultsFromFolderAsync(string testName, CancellationToken cancellationToken)
        {
            var diagnosticPath = Path.Combine(DataSourcePath, testName, "Diagnostic");

            if (!Directory.Exists(diagnosticPath))
                return [];

            var results = await ReadResultsFromFolderAsync(diagnosticPath, cancellationToken);

            return GetDiagnosticResult(results);
        }

        private async Task<IEnumerable<Result>> ReadResultsFromFolderAsync(string diagnosticPath, CancellationToken cancellationToken)
        {
            return (await Task.WhenAll(Directory.GetFiles(diagnosticPath, "*.json")
                            .Where(static x => !x.EndsWith("action.json", StringComparison.InvariantCultureIgnoreCase))
                            .Select(x => ReadResultsAsync(x, cancellationToken)))).SelectMany(static x => x);
        }


        private async Task<Dictionary<string, string>> ReadSourcesAsync(string testName, CancellationToken cancellationToken)
        {
            var sourcePath = Path.Combine(DataSourcePath, testName, "Source");

            return await ReadFilesAsync(sourcePath, cancellationToken);
        }
        private static readonly Regex ExpectedFolderRegex = new(@"\d+$", RegexOptions.Compiled);
        private async Task<IEnumerable<FixResult>> ReadExpectedSourcesAsync(string testName, CancellationToken cancellationToken)
        {
            var testPath = Path.Combine(DataSourcePath, testName);

            var expectedFolders = Directory.GetDirectories(testPath, "Expected*");

            return await Task.WhenAll(expectedFolders.Select(async x => 
            {
                var m = ExpectedFolderRegex.Match(x);
                var index = m.Success ? int.Parse(m.Value) : 0;

                return new FixResult(index, await ReadFilesAsync(x, cancellationToken));
            }));
        }

        private static async Task<Dictionary<string, string>> ReadFilesAsync(string sourcePath, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(sourcePath))
                return null;

            var sources = new Dictionary<string, string>();

            foreach (var file in Directory.GetFiles(sourcePath, "*.csx"))
            {
                var code = await File.ReadAllTextAsync(file, cancellationToken);
                var name = Path.GetFileName(file);
                sources.Add(name, code);
            }

            return sources;
        }

        class FixResult
        {
            public int Index { get; }

            public Dictionary<string, string> ExpectedSources { get; }

            /// <summary></summary>
            /// <param name="index"><see cref="Index"/></param>
            /// <param name="expectedSources"><see cref="ExpectedSources"/></param>
            public FixResult(int index, Dictionary<string, string> expectedSources)
            {
                Index = index;
                ExpectedSources = expectedSources;
            }
        }

        private async Task VerifyCSharpAsync(Dictionary<string, string> sources, DiagnosticResult[] expectedResults, params FixResult[] fixResults)
        {
            var analyzer = GetCSharpDiagnosticAnalyzer();
            var fix = GetCSharpCodeFixProvider();

            var originalProject = CreateProject(sources);

            var diagnostics = GetDiagnostics(originalProject, analyzer);
            VerifyDiagnosticResults(diagnostics, analyzer, expectedResults);

            foreach (var fixResult in fixResults)
            {
                var project = await ApplyFixAsync(originalProject, analyzer, fix, fixResult.Index);

                var expectedSources = fixResult.ExpectedSources;

                if (expectedSources == null || expectedSources.Count == 0)
                    return;

                var actualSources = new Dictionary<string, string>();

                foreach (var doc in project.Documents)
                {
                    var code = GetStringFromDocument(doc);
                    actualSources.Add(doc.Name, code);
                }

                Assert.True(actualSources.Keys.SequenceEqual(expectedSources.Keys));

                foreach (var item in actualSources)
                {
                    var actual = item.Value;
                    var newSource = expectedSources[item.Key];
                    Assert.Equal(newSource, actual);
                }
            }
        }

        private static async Task<Project> ApplyFixAsync(Project project, DiagnosticAnalyzer analyzer, CodeFixProvider fix, int fixIndex)
        {
            var diagnostics = GetDiagnostics(project, analyzer);
            var fixableDiagnostics = diagnostics.Where(d => fix.FixableDiagnosticIds.Contains(d.Id)).ToArray();

            var attempts = fixableDiagnostics.Length;

            for (int i = 0; i < attempts; i++)
            {
                var diag = fixableDiagnostics.First();
                var doc = project.Documents.FirstOrDefault(d => d.Name == diag.Location.SourceTree.FilePath);

                if (doc == null)
                {
                    fixableDiagnostics = fixableDiagnostics.Skip(1).ToArray();
                    continue;
                }

                var actions = new List<CodeAction>();
                var fixContext = new CodeFixContext(doc, diag, (a, d) => actions.Add(a), CancellationToken.None);
                await fix.RegisterCodeFixesAsync(fixContext);

                if (actions.Count == 0)
                {
                    break;
                }

                var codeAction = actions[fixIndex];

                var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
                var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
                project = solution.GetProject(project.Id);

                fixableDiagnostics = GetDiagnostics(project, analyzer)
                    .Where(d => fix.FixableDiagnosticIds.Contains(d.Id)).ToArray();

                if (fixableDiagnostics.Length == 0) break;
            }

            return project;
        }

        private static Diagnostic[] GetDiagnostics(Project project, DiagnosticAnalyzer analyzer)
        {
            var compilationWithAnalyzers = project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create(analyzer));
            var diagnostics = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        }

        protected static readonly IEnumerable<MetadataReference> References =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(JsonSerializer).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(HttpClient).Assembly.Location),
        ];

        protected virtual CSharpCompilationOptions CompilationOptions => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        protected Project CreateProject(Dictionary<string, string> sources)
        {
            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp);

            foreach (var reference in References)
            {
                solution = solution.AddMetadataReference(projectId, reference);
            }

            int count = 0;
            foreach (var source in sources)
            {
                var newFileName = source.Key;
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source.Value));
                count++;
            }

            var project = solution.GetProject(projectId)
                .WithCompilationOptions(CompilationOptions);
            return project;
        }

        #region read expected results from JSON file

        private IEnumerable<DiagnosticResult> GetDiagnosticResult(IEnumerable<Result> results)
        {
            var supportedDiagnostics = GetCSharpDiagnosticAnalyzer().SupportedDiagnostics;
            var analyzers = supportedDiagnostics.ToDictionary(x => x.Id);

            foreach (var r in results)
            {
                var diag = analyzers[r.Id];
                yield return new DiagnosticResult
                {
                    Id = r.Id,
                    Message = r.MessageArgs == null ? diag.MessageFormat.ToString() : string.Format(diag.MessageFormat.ToString(), (object[])r.MessageArgs),
                    Severity = r.Severity,
                    Locations = new[] { new DiagnosticResultLocation(r.Path ?? "Source.cs", r.Line, r.Column) },
                };
            }
        }

        private async Task<IEnumerable<Result>> ReadResultsAsync(string path, CancellationToken cancellationToken)
        {
            if (!File.Exists(path)) return [];

            try
            {
                using var stream = File.OpenRead(path);
                var result = await JsonSerializer.DeserializeAsync(stream, SerializationContext.Result, cancellationToken: cancellationToken);
                return [result];
            }
            catch
            {
                // backward compatibility
                using var stream = File.OpenRead(path);
                var results = await JsonSerializer.DeserializeAsync(stream, SerializationContext.ResultArray, cancellationToken: cancellationToken);
                return results;
            }
        }
        #endregion
        private static readonly ResultSerializationContext SerializationContext = new(
            new JsonSerializerOptions {
                Converters = { new JsonStringEnumConverter()
            }
        });
    }
    internal class Result
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("severity")]
        public DiagnosticSeverity Severity { get; set; }

        [JsonPropertyName("line")]
        public int Line { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("message-args")]
        public string[] MessageArgs { get; set; }
    }
    [JsonSerializable(typeof(Result))]
    [JsonSerializable(typeof(Result[]))]
    internal partial class ResultSerializationContext : JsonSerializerContext
    {

    }
}