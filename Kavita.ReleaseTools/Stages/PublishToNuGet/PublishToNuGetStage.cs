using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.PublishToNuGet;

public class PublishToNuGetStage(ILogger<PublishToNuGetStage> logger, IProcessRunner runner) : ConfiguredStage<PublishToNuGetConfiguration>(logger)
{
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
        await new PublishNuGetPackageCommand(runner, config).RunAsync(ctx, ct);
    }
}
