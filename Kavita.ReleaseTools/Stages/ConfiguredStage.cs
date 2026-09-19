using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using Kavita.ReleaseTools.Stages.ParseReleaseTypes;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;
using ValidationContext = Kavita.ReleaseTools.Models.ValidationContext;

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

        return
        [
            .. ValidationIssueFactory.FromAnnotations(Name, configuration),
            .. Validate(ctx, configuration)
        ];
    }

    public async Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        var configuration = GetConfiguration(ctx.Configuration);
        if (configuration is null)
        {
            logger.LogTrace("Skipping stage as no configuration was found,");
            return;
        }

        if (configuration.Disabled)
        {
            logger.LogDebug("Skipping disabled stage");
            return;
        }

        if (configuration.ReleaseTypes.Count > 0
            && !ctx.ReleaseTypes.Intersect(configuration.ReleaseTypes).Any()
            && this is not ParseReleaseTypesStage) // Hardcoded bypass as this parses the releases
        {
            logger.LogInformation("Skipping stage as none of {ReleaseTypes} are active", configuration.ReleaseTypes);
            return;
        }

        await ExecuteAsync(ctx, configuration, ct);
    }

    protected abstract TConfiguration? GetConfiguration(ReleaseConfiguration configuration);

    protected abstract IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, TConfiguration config);

    protected abstract Task ExecuteAsync(ExecutionContext ctx, TConfiguration config, CancellationToken ct);

    protected ValidationIssue Issue(string message) => new()
    {
        StageName = Name,
        Message = message,
    };

}
