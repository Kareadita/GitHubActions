using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Kavita.ReleaseTools.Commands.ExternalCommands;

/// <summary>
/// <see cref="IProcessRunner"/> backed by <see cref="System.Diagnostics.Process"/>
/// </summary>
/// <remarks>
/// Output is streamed to the log as it is produced, so a long build is still visible live, and
/// captured so a failure can be reported with the lines that explain it
/// </remarks>
public class ProcessRunner: IProcessRunner
{
    public async Task<ProcessResult> RunAsync(string executable, IReadOnlyList<string> arguments,
        string? workingDirectory, LogLevel logLevel, CancellationToken ct)
    {
        var logEventLevel = logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.None => LogEventLevel.Verbose,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };


        var logger = Log.ForContext("SourceContext", executable);

        var standardOutput = new StringBuilder();
        var standardError = new StringBuilder();

        using var process = new Process();
        process.StartInfo.FileName = executable;
        process.StartInfo.WorkingDirectory = workingDirectory ?? string.Empty;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        process.StartInfo.UseShellExecute = false;

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is null) return;

            standardOutput.AppendLine(args.Data);
            logger.Write(logEventLevel, "{Line}", args.Data);
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is null) return;

            standardError.AppendLine(args.Data);
            logger.Write(logEventLevel, "{Line}", args.Data);
        };

        logger.Debug("Running {Executable} {Arguments} in {WorkingDirectory}", executable, string.Join(' ', arguments),
            workingDirectory ?? "(current directory)");

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(ct);

            // WaitForExitAsync returns before the output handlers have drained
            process.WaitForExit();
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        return new ProcessResult
        {
            Executable = executable,
            Arguments = arguments,
            ExitCode = process.ExitCode,
            StandardOutput = standardOutput.ToString(),
            StandardError = standardError.ToString(),
        };
    }
}
