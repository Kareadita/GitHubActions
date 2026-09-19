using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;

namespace Kavita.ReleaseTools.Tests;

/// <summary>
/// An <see cref="IProcessRunner"/> that records what it was asked to run, so a stage can be tested
/// without spawning anything
/// </summary>
public sealed class FakeProcessRunner(Func<string, IReadOnlyList<string>, ProcessResult>? responder = null): IProcessRunner
{
    public List<ProcessInvocation> Invocations { get; } = [];

    public Task<ProcessResult> RunAsync(string executable, IReadOnlyList<string> arguments, string? workingDirectory,
        LogLevel logLevel, CancellationToken ct)
    {
        Invocations.Add(new ProcessInvocation(executable, arguments.ToArray(), workingDirectory));

        return Task.FromResult(responder?.Invoke(executable, arguments) ?? Succeeded(executable, arguments));
    }

    public static ProcessResult Succeeded(string executable, IReadOnlyList<string> arguments) => new()
    {
        Executable = executable,
        Arguments = arguments,
        ExitCode = 0,
        StandardOutput = string.Empty,
        StandardError = string.Empty,
    };

    public static ProcessResult Failed(string executable, IReadOnlyList<string> arguments, string standardError) => new()
    {
        Executable = executable,
        Arguments = arguments,
        ExitCode = 1,
        StandardOutput = string.Empty,
        StandardError = standardError,
    };
}

public record ProcessInvocation(string Executable, IReadOnlyList<string> Arguments, string? WorkingDirectory)
{
    public string CommandLine => $"{Executable} {string.Join(' ', Arguments)}";
}
