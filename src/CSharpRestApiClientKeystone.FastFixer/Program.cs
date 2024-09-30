using System.CommandLine;

namespace CSharpRestApiClientKeystone.FastFixer;
public static class Program
{
    static async Task<int> Main(string[] args)
    {
        var fileArgument = new Argument<List<string>>("filePath", "The file path to the source file to fix") {
            Arity = ArgumentArity.OneOrMore,
        };

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
        return await rootCommand.InvokeAsync(args);
    }
}

