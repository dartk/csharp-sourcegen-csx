using System.Collections.Immutable;
using Microsoft.CodeAnalysis;


namespace CSharp.SourceGen.Csx;


[Generator(LanguageNames.CSharp)]
public class CsxIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalFiles = context.AdditionalTextsProvider
            .Select(static (file, token) =>
            {
                var source = file.GetText(token)?.ToString() ?? string.Empty;
                return new AdditionalFile(file.Path, source);
            })
            .Where(static x => x.IsNotEmpty);

        var csxScripts = additionalFiles.Where(file =>
            file.FilePath.EndsWith(".csx")
            && file.FileName[0] != '_'
            && file.IsNotEmpty);

        // Scripts can depend on other additional files. To call the generator on this files' changes
        // we need to include them into the cache
        var csxScriptsWithDependencies = csxScripts.Combine(additionalFiles.Collect())
            .Select(static (arg, _) =>
            {
                var (csx, additionalFiles) = arg;
                var builder =
                    ImmutableArray.CreateBuilder<AdditionalFile>(additionalFiles.Length);
                foreach (var file in additionalFiles)
                {
                    if (csx.Text.Contains(file.FileName))
                    {
                        builder.Add(file);
                    }
                }

                return (csxToExecute: csx, builder.ToImmutable());
            });

        context.RegisterSourceOutput(csxScriptsWithDependencies, (productionContext, script) =>
            CompileScript(productionContext, script.csxToExecute.FilePath));
    }


    private static void CompileScript(SourceProductionContext context, string scriptFile)
    {
        var source =
            $"// Generated from '{Path.GetFileName(scriptFile)}'"
            + Environment.NewLine
            + ScriptRunner.Run(scriptFile, context.ReportDiagnostic, context.CancellationToken);
        context.AddSource(Path.GetFileName(scriptFile) + ".out", source);
    }


    private readonly record struct AdditionalFile(string FilePath, string Text)
    {
        public string FileName => Path.GetFileName(this.FilePath);
        public bool IsNotEmpty => !string.IsNullOrWhiteSpace(this.Text);
    }
}