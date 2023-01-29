using System.Diagnostics;
using Microsoft.CodeAnalysis;


namespace CSXSourceGenerator;


[Generator]
public class CsxGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }


    public void Execute(GeneratorExecutionContext context)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet-script",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        foreach (var file in context.AdditionalFiles)
        {
            if (Path.GetExtension(file.Path) != ".csx")
            {
                continue;
            }

            startInfo.Arguments = file.Path;
            startInfo.WorkingDirectory = Path.GetDirectoryName(file.Path);

            var process = new Process { StartInfo = startInfo };
            process.Start();
            var source = process.StandardOutput.ReadToEnd();
            var errors = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(errors))
            {
                throw new Exception(errors);
            }

            context.AddSource(Path.GetFileNameWithoutExtension(file.Path), source);
        }
    }
}