using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Compression;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpRestApiClientKeystone.FastFixer;

public class FixFileCommandHandler : ICommandHandler
{
    public required Argument<List<string>> FilePathsArgument { get; init; }
    public required Option<List<string>> NugetPackagesOption { get; init; }
    public required Option<List<string>> DeffectsToFixOption { get; init; }
    public int Invoke(InvocationContext context)
    {
        throw new InvalidOperationException("Use InvokeAsync instead");
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var filePaths = context.ParseResult.GetValueForArgument(FilePathsArgument).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var nugetPackages = (context.ParseResult.GetValueForOption(NugetPackagesOption) ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var deffectsToFix = (context.ParseResult.GetValueForOption(DeffectsToFixOption) ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var cancellationToken = context.BindingContext.GetService(typeof(CancellationToken)) is CancellationToken token ? token : CancellationToken.None;

        var missingFiles = filePaths.Where(x => !Path.Exists(x)).ToList();
        if (missingFiles.Count > 0)
        {
            Console.WriteLine($"ERROR: Files {string.Join(',', filePaths)} not found");
            return 1;
        }

        var (loadedAnalyzers, loadedFixProviders) = await LoadNugetPackagesAsync(nugetPackages, cancellationToken);
        var analyzersToApply = loadedAnalyzers.Where(x => deffectsToFix.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        var fixProvidersToApply = loadedFixProviders.Where(x => deffectsToFix.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var originalSources = new Dictionary<string, string>(filePaths.Count);
        foreach (var file in filePaths)
        {
            originalSources.Add(file, await File.ReadAllTextAsync(file, cancellationToken));
        }

        throw new NotImplementedException();
    }
    private static readonly HttpClient HttpClient = new();

    private static async Task<(Dictionary<string, DiagnosticAnalyzer>, Dictionary<string, CodeFixProvider>)> LoadNugetPackagesAsync(List<string> nugetPackages, CancellationToken cancellationToken)
    {
        var resultAnalyzers = new Dictionary<string, DiagnosticAnalyzer>(StringComparer.OrdinalIgnoreCase);
        var resultFixProviders = new Dictionary<string, CodeFixProvider>(StringComparer.OrdinalIgnoreCase);
        Directory.CreateDirectory("packages");
        foreach (var package in nugetPackages)
        {
            var packageTargetPath = Path.Combine("packages", package.Replace(':', Path.DirectorySeparatorChar));
            if (!Directory.Exists(packageTargetPath))
            {
                using var nuPkg = await HttpClient.GetStreamAsync($"https://www.nuget.org/api/v2/package/{package.Replace(':', '/')}", cancellationToken);
                using var zip = new ZipArchive(nuPkg);
                zip.ExtractToDirectory(packageTargetPath, true);
            }
            foreach (var dll in Directory.GetFiles(packageTargetPath, "*.dll", SearchOption.AllDirectories))
            {
                var assembly = Assembly.LoadFrom(dll);
                var analyzers = assembly.GetTypes().Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract).ToArray();
                foreach (var analyzerClass in analyzers)
                {
                    var analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerClass);
                    foreach (var diagnostic in analyzer.SupportedDiagnostics)
                    {
                        resultAnalyzers.TryAdd(diagnostic.Id, analyzer);
                    }
                }
                var fixProviders = assembly.GetTypes().Where(t => typeof(CodeFixProvider).IsAssignableFrom(t) && !t.IsAbstract).ToArray();
                foreach (var fixProviderClass in fixProviders)
                {
                    var fixProvider = (CodeFixProvider)Activator.CreateInstance(fixProviderClass);
                    foreach (var diagnostic in fixProvider.FixableDiagnosticIds)
                    {
                        resultFixProviders.TryAdd(diagnostic, fixProvider);
                    }
                }

            }
        }
        return (resultAnalyzers, resultFixProviders);
    }

    // private async Task FixCSharpAsync(Dictionary<string, string> sources, DiagnosticResult[] expectedResults, params FixResult[] fixResults)
    // {
    //     var analyzer = new VSTHRD111UseConfigureAwaitAnalyzer();
    //     var fix = GetCSharpCodeFixProvider();

    //     var originalProject = CreateProject(sources);

    //     var diagnostics = GetDiagnostics(originalProject, analyzer);

    //     foreach (var fixResult in fixResults)
    //     {
    //         var project = await ApplyFixAsync(originalProject, analyzer, fix, fixResult.Index);

    //         var expectedSources = fixResult.ExpectedSources;

    //         if (expectedSources == null || expectedSources.Count == 0)
    //             return;

    //         var actualSources = new Dictionary<string, string>();

    //         foreach (var doc in project.Documents)
    //         {
    //             var code = GetStringFromDocument(doc);
    //             actualSources.Add(doc.Name, code);
    //         }

    //         foreach (var item in actualSources)
    //         {
    //             var actual = item.Value;
    //             var newSource = expectedSources[item.Key];
    //             Assert.Equal(newSource, actual);
    //         }
    //     }
    // }
    #region ImportedFromUnitTests
    protected static CSharpCompilationOptions CompilationOptions => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
    private static readonly DirectoryInfo _coreDir = Directory.GetParent(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory());
    protected static readonly IEnumerable<MetadataReference> References =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Net.Http.HttpContent).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Object).GetTypeInfo().Assembly.Location),
        MetadataReference.CreateFromFile(_coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll")
    ];
    private const string TestProjectName = "TestProject";
    private static Project CreateProject(Dictionary<string, string> sources)
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
    private static Diagnostic[] GetDiagnostics(Project project, DiagnosticAnalyzer analyzer)
    {
        var compilationWithAnalyzers = project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create(analyzer));
        var diagnostics = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
        return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
    }
    #endregion
}
