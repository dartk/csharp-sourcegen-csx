using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;


namespace CSXSourceGenerator;


[Generator]
public class CsxGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }


    public void Execute(GeneratorExecutionContext context)
    {
        var defaultEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet-script",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardErrorEncoding = defaultEncoding
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
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var errors = process.StandardError.ReadToEnd();
                var match = ExceptionRegex.Match(errors);
                if (match.Success)
                {
                    var message = match.Groups["message"].Value;
                    var line = int.Parse(match.Groups["line"].Value);
                    
                    var linePositionStart = new LinePosition(line - 1, 0);
                    var linePositionEnd = new LinePosition(line , 0);
                    var linePositionSpan = new LinePositionSpan(linePositionStart, linePositionEnd);
                    var location = Location.Create(file.Path, default, linePositionSpan);
                    context.ReportDiagnostic(Diagnostic.Create(
                        ScriptRuntimeException,
                        location,
                        message));
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(ScriptExecutionError, Location.None, file.Path, errors));
                }

                return;
            }

            var source = process.StandardOutput.ReadToEnd();
            context.AddSource(Path.GetFileNameWithoutExtension(file.Path), source);
        }
    }


    private static readonly DiagnosticDescriptor ScriptRuntimeException = new(
        id: "CSXGEN002",
        title: "C# script threw exception",
        messageFormat: "{0}",
        category: "CsxSourceGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    private static readonly DiagnosticDescriptor ScriptExecutionError = new(
        id: "CSXGEN001",
        title: "Couldn't execute C# script",
        messageFormat: "'{0}' execution ended with errors." + Environment.NewLine + "{1}",
        category: "CsxSourceGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    
    private static readonly Regex ExceptionRegex =
        new ($@".*Exception: (?<message>.*)\r?\n? *at .*:line (?<line>[\d]+)");
}