using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.RunScript;

public class RunScriptStage(ILogger<RunScriptStage> logger, IProcessRunner runner) : ConfiguredStage<RunScriptConfiguration>(logger)
{
    public override string Name => nameof(RunScriptStage);
    protected override RunScriptConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.RunScript;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, RunScriptConfiguration config)
    {
        return [];
    }

    protected override Task ExecuteAsync(ExecutionContext ctx, RunScriptConfiguration config, CancellationToken ct)
    {
        return new ProcessCommand.Builder(runner)
            .WithLogLevel(config.LogLevel)
            .WithWorkingDirectory(config.WorkingDirectory)
            .WithExecutable(config.Executable)
            .WithArguments([..config.Arguments])
            .Build()
            .RunAsync(ctx, ct);
    }
}
