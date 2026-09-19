using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Commands.Git;
using Kavita.ReleaseTools.Models;
using LibGit2Sharp;
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
        var message = config.CommitMessage ?? $"Release changes for {ctx.ReleaseVersion?.ToString() ?? "Unknown"}";
        var commitCommand = new GitCommitCommand.Builder()
            .WithCommitMessage(message)
            .WithFiles(config.Files)
            .Build();

        try
        {
            await commitCommand.RunAsync(ctx, ct);
        }
        catch (EmptyCommitException)
        {
            // Every configured file already matches what is committed, so there is no commit to make
            // and nothing worth pushing
            logger.LogInformation("No changes in {Files}, nothing to flush", string.Join(", ", config.Files));
            return;
        }

        logger.LogInformation("Created commit {CommitMessage} for {FileCount} file(s)", message, config.Files.Count);

        var pushCommand = new GitPushCommand();
        //await pushCommand.RunAsync(ctx, ct);
    }
}
