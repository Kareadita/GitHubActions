using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Commands.Git;
using Kavita.ReleaseTools.Models;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.FlushGitChanges;

public partial class FlushGitChangesStage(ILogger<FlushGitChangesStage> logger): ConfiguredStage<FlushGitChangesConfiguration>(logger)
{
    public override string Name => nameof(FlushGitChangesStage);

    private static readonly Regex BranchRegex = MyBranchRegex();

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, FlushGitChangesConfiguration config)
    {
        if (!BranchRegex.IsMatch(config.Branch))
        {
            return [Issue($"Invalid branch name: {config.Branch}")];
        }

        return [];
    }

    protected override FlushGitChangesConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.FlushGitChanges;
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, FlushGitChangesConfiguration config, CancellationToken ct)
    {
        await CommitChanges(ctx, config, ct);

        var pushCommand = new GitPushCommand.Builder()
            .WithBranchName(config.Branch)
            .Build();

        await pushCommand.RunAsync(ctx, ct);
    }

    private async Task CommitChanges(ExecutionContext ctx, FlushGitChangesConfiguration config, CancellationToken ct)
    {
        if (config.Files.Count == 0)
        {
            logger.LogTrace("No changes in {Files}, nothing to commit", config.Files.Count);
            return;
        }

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
            logger.LogWarning("Files were configured to be committed, but contained no changed.");
            return;
        }

        logger.LogInformation("Created commit {CommitMessage} for {FileCount} file(s)", message, config.Files.Count);

    }

    [GeneratedRegex(@"^(?!\/|.*(?:[/.]\.|\/\/|@\{|\\))[^\x00-\x20~^:?*\[\\]+(?<!\.)(?<!\.lock)(?<!\/)$", RegexOptions.Compiled
    )]
    private static partial Regex MyBranchRegex();
}
