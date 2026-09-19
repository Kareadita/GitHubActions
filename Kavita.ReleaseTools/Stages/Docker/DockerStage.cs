using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Models;
using Kavita.ReleaseTools.Stages.VersionBump;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.Docker;

public class DockerStage(ILogger<DockerStage> logger, IProcessRunner runner) : ConfiguredStage<DockerConfiguration>(logger)
{

    private const string Docker = "docker";

    public override string Name => nameof(DockerStage);

    protected override DockerConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.Docker;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, DockerConfiguration config)
    {
        return [];
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, DockerConfiguration config, CancellationToken ct)
    {
        if (ctx.ReleaseVersion is null)
            throw new ExecutionException($"Cannot push images without a version. Did you configure {nameof(VersionBumpStage)}");

        var version = ctx.ReleaseVersion?.ToString();
        var tags = config.Tags
            .Where(kv => ctx.ReleaseTypes.Contains(kv.Key))
            .SelectMany(kv => kv.Value)
            .Select(t => t.Replace("{Version}", version ?? string.Empty))
            .ToArray();

        var allTags = config.Images
            .SelectMany(img => tags.Select(t => $"{img}:{t}"))
            .ToArray();

        await new ProcessCommand.Builder(runner)
            .WithLogLevel(config.LogLevel)
            .WithExecutable(Docker)
            .WithArguments("buildx", "build")
            .WithRepeatedArgument("--tag", allTags)
            .AppendArgumentIf(config.Platforms.Count > 0, "--platform", string.Join(',', config.Platforms))
            .AppendArgumentIf(!string.IsNullOrEmpty(config.File), "--file", config.File)
            .AppendArgumentIf(config.Push && !ctx.Configuration.DryRun, "--push")
            .AppendArgumentIf(config.Load, "--load")
            .WithArguments(config.Context)
            .Build()
            .RunAsync(ctx, ct);
    }
}
