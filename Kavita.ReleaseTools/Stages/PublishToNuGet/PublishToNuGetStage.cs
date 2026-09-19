using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Models;
using Kavita.ReleaseTools.Stages.VersionBump;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.PublishToNuGet;

public class PublishToNuGetStage(ILogger<PublishToNuGetStage> logger, IProcessRunner runner) : ConfiguredStage<PublishToNuGetConfiguration>(logger)
{
    private const string Dotnet = "dotnet";

    public override string Name => nameof(PublishToNuGetStage);

    protected override PublishToNuGetConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.PublishToNuGet;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, PublishToNuGetConfiguration config)
    {
        return [];
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, PublishToNuGetConfiguration config, CancellationToken ct)
    {
        if (ctx.ReleaseVersion is null)
            throw new ExecutionException($"Cannot publish to NuGet without a version. Did you configure {nameof(VersionBumpStage)}");

        await new ProcessCommand.Builder(runner)
            .WithExecutable(Dotnet)
            .WithArguments("nuget", "push", $"{config.OutputDirectory}/*.nupkg", "--api-key", config.ApiKey, "--source", config.Source)
            .AppendArgumentIf(config.SkipDuplicate, "--skip-duplicate")
            .Build()
            .RunAsync(ctx, ct);
    }
}
