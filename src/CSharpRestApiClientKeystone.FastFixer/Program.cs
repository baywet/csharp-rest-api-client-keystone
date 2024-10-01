using System.CommandLine;
using System.Threading;

namespace CSharpRestApiClientKeystone.FastFixer;
public static class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = GetRootCommand();
        return await rootCommand.InvokeAsync(args).ConfigureAwait(true);
    }
    private static RootCommand GetRootCommand()
    {
        var csprojArgument = new Argument<string>("csprojPath", "The file path to the csproj for files to fix") {
            Arity = ArgumentArity.ExactlyOne,
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
            csprojArgument,
            nugetPackagesOption,
            deffectsToFixOption,
        };

        var fileCommandHandler = new FixFileCommandHandler
        {
            CsProjPathArgument = csprojArgument,
            NugetPackagesOption = nugetPackagesOption,
            DeffectsToFixOption = deffectsToFixOption,
        };
        rootCommand.Handler = fileCommandHandler;
        return rootCommand;
    }
}

