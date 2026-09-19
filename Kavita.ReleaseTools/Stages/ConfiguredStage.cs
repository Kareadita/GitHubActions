using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages;

public abstract class ConfiguredStage<TConfiguration>(ILogger logger): IStage
where TConfiguration : IStageConfiguration
{
    public abstract string Name { get; }

    public IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx)
    {
        var configuration = GetConfiguration(ctx.Configuration);
        if (configuration is null) return [];

        if (configuration.Disabled && !ctx.Configuration.ValidateDisabledStages) return [];

        return Validate(ctx, configuration);
    }

    public async Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        var configuration = GetConfiguration(ctx.Configuration);
        if (configuration is null || configuration.Disabled)
        {
            logger.LogDebug("Skipping stage as no configuration was found, or the stage has been disabled");
            return;
        }

        await ExecuteAsync(ctx, configuration, ct);
    }

    protected abstract TConfiguration? GetConfiguration(ReleaseConfiguration configuration);

    protected abstract IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, TConfiguration config);

    protected abstract Task ExecuteAsync(ExecutionContext ctx, TConfiguration config, CancellationToken ct);


}
