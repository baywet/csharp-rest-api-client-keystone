using System;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Compression;
using System.Reflection;
using System.Text;

using Buildalyzer;
using Buildalyzer.Construction;
using Buildalyzer.Workspaces;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace CSharpRestApiClientKeystone.FastFixer;

public class FixFileCommandHandler : ICommandHandler
{
    public required Argument<string> CsProjPathArgument { get; init; }
    public required Option<List<string>> NugetPackagesOption { get; init; }
    public required Option<List<string>> DeffectsToFixOption { get; init; }
    public int Invoke(InvocationContext context)
    {
        throw new InvalidOperationException("Use InvokeAsync instead");
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var csprojPath = context.ParseResult.GetValueForArgument(CsProjPathArgument);
        var nugetPackages = (context.ParseResult.GetValueForOption(NugetPackagesOption) ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var deffectsToFix = (context.ParseResult.GetValueForOption(DeffectsToFixOption) ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var cancellationToken = context.BindingContext.GetService(typeof(CancellationToken)) is CancellationToken token ? token : CancellationToken.None;

        if (!Path.Exists(csprojPath))
        {
            Console.WriteLine($"ERROR: Files {csprojPath} not found");
            return 1;
        }

        var (loadedAnalyzers, loadedFixProviders) = await LoadNugetPackagesAsync(nugetPackages, cancellationToken);
        var analyzersToApply = loadedAnalyzers.Where(x => deffectsToFix.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        var fixProvidersToApply = loadedFixProviders.Where(x => deffectsToFix.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var originalProject = CreateProject(csprojPath, analyzersToApply);
        foreach(var analyzerToApply in analyzersToApply)
        {
            await FixCSharpAsync(originalProject, analyzerToApply.Value, fixProvidersToApply[analyzerToApply.Key], cancellationToken);
        }
        Console.WriteLine("All done");
        return 0;
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

    private static async Task FixCSharpAsync(Project originalProject, DiagnosticAnalyzer analyzer, CodeFixProvider fix, CancellationToken cancellationToken)
    {
        var project = await ApplyFixAsync(originalProject, analyzer, fix, cancellationToken).ConfigureAwait(false);
        foreach (var doc in project.Documents)
        {
            var code = await GetStringFromDocumentAsync(doc, cancellationToken).ConfigureAwait(false);
            if (doc.FilePath is null)
            {
                Console.WriteLine($"ERROR: Document {doc.Name} has no file path");
                continue;
            }
            await File.WriteAllTextAsync(doc.FilePath, code, new UTF8Encoding(true), cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"Fixed {doc.FilePath}");
        }
    }
    #region ImportedFromUnitTests
    private static async Task<string> GetStringFromDocumentAsync(Document document, CancellationToken cancellationToken)
    {
        var simplifiedDoc = await Simplifier.ReduceAsync(document, Simplifier.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);
        var root = await simplifiedDoc.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace, cancellationToken: cancellationToken);
        return root.GetText().ToString();
    }
    private static async Task<Project> ApplyFixAsync(Project project, DiagnosticAnalyzer analyzer, CodeFixProvider fix, CancellationToken cancellationToken)
    {
        var diagnostics = await GetDiagnosticsAsync(project, analyzer, cancellationToken).ConfigureAwait(false);
        var fixableDiagnostics = diagnostics.Where(d => fix.FixableDiagnosticIds.Contains(d.Id)).ToArray();

        var attempts = fixableDiagnostics.Length;

        for (int i = 0; i < attempts; i++)
        {
            var diag = fixableDiagnostics.First();
            var doc = project.Documents.FirstOrDefault(d => d.FilePath == diag.Location.SourceTree.FilePath);

            if (doc == null)
            {
                fixableDiagnostics = fixableDiagnostics.Skip(1).ToArray();
                continue;
            }

            var actions = new List<CodeAction>();
            var fixContext = new CodeFixContext(doc, diag, (a, d) => actions.Add(a), CancellationToken.None);
            await fix.RegisterCodeFixesAsync(fixContext).ConfigureAwait(false);

            if (actions.Count == 0)
            {
                break;
            }

            foreach (var codeAction in actions.Where(x => x.Title.Contains("false", StringComparison.OrdinalIgnoreCase)))
            {
                var operations = await codeAction.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
                var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
                project = solution.GetProject(project.Id);

                fixableDiagnostics = (await GetDiagnosticsAsync(project, analyzer, cancellationToken).ConfigureAwait(false))
                    .Where(d => fix.FixableDiagnosticIds.Contains(d.Id)).ToArray();

                if (fixableDiagnostics.Length == 0) break;
            }
        }

        return project;
    }
    private static Project CreateProject(string csProjPath, Dictionary<string, DiagnosticAnalyzer> analyzersToApply)
    {
        var manager = new AnalyzerManager();
        var analyzer = manager.GetProject(csProjPath);
        // var results = analyzer.Build();
        // var sourceFiles = results.First().SourceFiles;
        using var workspace = manager.GetWorkspace();
        var firstProject = workspace.CurrentSolution.Projects.First();
        return !firstProject.AnalyzerReferences.Any(static x => x.Display.Equals("Microsoft.VisualStudio.Threading.Analyzers"))
            ? firstProject.AddAnalyzerReference(new AnalyzerImageReference(analyzersToApply.Select(static x => x.Value).ToImmutableArray()))
            : firstProject;
    }
    private static async Task<Diagnostic[]> GetDiagnosticsAsync(Project project, DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
    {
        var compilationWithAnalyzers = (await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false)).WithAnalyzers(ImmutableArray.Create(analyzer));
        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
        return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
    }
    #endregion
}
