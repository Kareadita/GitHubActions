using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Commands.ExternalCommands;

/// <summary>
/// Runs a single external process
/// </summary>
public class ProcessCommand: RunnableCommand
{
    private readonly IProcessRunner _runner;
    private readonly string _executable;
    private readonly IReadOnlyList<string> _arguments;
    private readonly string? _workingDirectory;
    private readonly LogLevel _logLevel;

    private ProcessCommand(IProcessRunner runner, string executable, IReadOnlyList<string> arguments, string? workingDirectory, LogLevel logLevel): base(true)
    {
        _runner = runner;
        _executable = executable;
        _arguments = arguments;
        _workingDirectory = workingDirectory;
        _logLevel = logLevel;
    }

    public override string Name => $"{_executable} {string.Join(' ', _arguments)}";

    protected override async Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        var result = await _runner.RunAsync(_executable, _arguments, _workingDirectory, _logLevel, ct);
        result.ThrowIfFailed();
    }

    public class Builder(IProcessRunner runner): ICommandBuilder
    {
        private string _executable = string.Empty;
        private List<string> _arguments = [];
        private string? _workingDirectory;
        private LogLevel _logLevel = LogLevel.Trace;

        public Builder WithExecutable(string executable)
        {
            _executable = executable;
            return this;
        }

        public Builder WithArguments(params string[] arguments)
        {
            _arguments = [.._arguments, ..arguments];
            return this;
        }

        public Builder WithRepeatedArgument(string argument, string[] values)
        {
            foreach (var value in values)
            {
                _arguments.Add(argument);
                _arguments.Add(value);
            }

            return this;
        }

        public Builder AppendArgumentIf(bool condition, params string[] argument)
        {
            if (condition)
                _arguments.AddRange(argument);

            return this;
        }

        public Builder WithWorkingDirectory(string? workingDirectory)
        {
            _workingDirectory = workingDirectory;
            return this;
        }

        public Builder WithLogLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
            return this;
        }

        public ICommand Build()
        {
            if (string.IsNullOrWhiteSpace(_executable))
            {
                throw new ArgumentException("Executable cannot be null or empty");
            }

            return new ProcessCommand(runner, _executable, _arguments, _workingDirectory,  _logLevel);
        }
    }
}
