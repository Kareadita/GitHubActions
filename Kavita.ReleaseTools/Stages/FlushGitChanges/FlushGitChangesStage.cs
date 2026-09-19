using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Commands.Git;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.FlushGitChanges;

public class FlushGitChangesStage(ILogger<FlushGitChangesStage> logger): ConfiguredStage<FlushGitChangesConfiguration>(logger)
{
    public override string Name => nameof(FlushGitChangesStage);

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, FlushGitChangesConfiguration config)
    {
        return [];
    }

    protected override FlushGitChangesConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.FlushGitChanges;
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, FlushGitChangesConfiguration config, CancellationToken ct)
    {
        var commitCommand = new GitCommitCommand.Builder()
            .WithCommitMessage(config.CommitMessage ?? $"Release changes for {ctx.ReleaseVersion?.ToString() ?? "Unknown"}")
            .WithFiles(config.Files)
            .Build();
        var pushCommand = new GitPushCommand();

        await commitCommand.RunAsync(ctx, ct);
        //await pushCommand.RunAsync(ctx, ct);
    }
}
