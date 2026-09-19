using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Commands.Git;
using Kavita.ReleaseTools.Models;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.FlushGitChanges;

public class FlushGitChangesStage: ConfiguredStage<FlushGitChangesConfiguration>
{
    public override string Name => nameof(FlushGitChangesStage);

    public override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx)
    {
        return [];
    }

    protected override FlushGitChangesConfiguration? GetConfiguration(ExecutionContext ctx)
    {
        return ctx.Configuration.FlushGitChanges;
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, FlushGitChangesConfiguration config, CancellationToken ct)
    {
        var commitCommand = new GitCommitCommand.Builder()
            .WithCommitMessage(config.CommitMessage ?? $"Release changes for {ctx.ReleaseVersion?.ToString() ?? "Unknown"}")
            .WithFiles(config.Files)
            .Build();
        var pushCommand = new GitPushCommand();

        await commitCommand.RunAsync(ctx, ct);
        await pushCommand.RunAsync(ctx, ct);
    }
}
