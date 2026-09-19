using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
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

    private ProcessCommand(IProcessRunner runner, string executable, IReadOnlyList<string> arguments, string? workingDirectory): base(true)
    {
        _runner = runner;
        _executable = executable;
        _arguments = arguments;
        _workingDirectory = workingDirectory;
    }

    public override string Name => $"{_executable} {string.Join(' ', _arguments)}";

    protected override async Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        var result = await _runner.RunAsync(_executable, _arguments, _workingDirectory, ct);
        result.ThrowIfFailed();
    }

    public class Builder(IProcessRunner runner): ICommandBuilder
    {
        private string _executable = string.Empty;
        private List<string> _arguments = [];
        private string? _workingDirectory;

        public Builder WithExecutable(string executable)
        {
            _executable = executable;
            return this;
        }

        public Builder WithArguments(params string[] arguments)
        {
            _arguments = [..arguments];
            return this;
        }

        public Builder AppendArgumentIf(bool condition, string argument)
        {
            if (condition)
                _arguments.Add(argument);

            return this;
        }

        public Builder WithWorkingDirectory(string? workingDirectory)
        {
            _workingDirectory = workingDirectory;
            return this;
        }

        public ICommand Build()
        {
            if (string.IsNullOrWhiteSpace(_executable))
            {
                throw new ArgumentException("Executable cannot be null or empty");
            }

            return new ProcessCommand(runner, _executable, _arguments, _workingDirectory);
        }
    }
}
