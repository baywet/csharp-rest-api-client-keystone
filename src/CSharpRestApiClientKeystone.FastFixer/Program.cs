using System.CommandLine;
using System.Threading;

namespace CSharpRestApiClientKeystone.FastFixer;
public static class Program
{
    static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };
        using var siStream = Console.OpenStandardInput();
        using var sr = new StreamReader(siStream);
        var input = await sr.ReadToEndAsync(cts.Token);
        var rootCommand = GetRootCommand(input);
        return await rootCommand.InvokeAsync(args);
    }
    private static RootCommand GetRootCommand(string inputValueForFilePath)
    {
        var inputProvided = !string.IsNullOrWhiteSpace(inputValueForFilePath);
        var fileArgument = new Argument<List<string>>("filePath", "The file path to the source file to fix") {
            Arity = inputProvided ? ArgumentArity.ZeroOrMore : ArgumentArity.OneOrMore,
        };
        if (inputProvided)
            fileArgument.SetDefaultValue(inputValueForFilePath.Split([' ', '\n', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        var nugetPackagesOption = new Option<List<string>>("--nuget-packages", "The nuget packages to use for the fixer") {
            Arity = ArgumentArity.ZeroOrMore,
        };
        nugetPackagesOption.SetDefaultValue(new List<string> { "Microsoft.VisualStudio.Threading.Analyzers:17.11.20" });

        var deffectsToFixOption = new Option<List<string>>("--deffects-to-fix", "The deffects to fix") {
            Arity = ArgumentArity.ZeroOrMore,
        };
        deffectsToFixOption.SetDefaultValue(new List<string> { "VSTHRD111" });

        var rootCommand = new RootCommand
        {
            fileArgument,
            nugetPackagesOption,
            deffectsToFixOption,
        };

        var fileCommandHandler = new FixFileCommandHandler
        {
            FilePathsArgument = fileArgument,
            NugetPackagesOption = nugetPackagesOption,
            DeffectsToFixOption = deffectsToFixOption,
        };
        rootCommand.Handler = fileCommandHandler;
        return rootCommand;
    }
}

