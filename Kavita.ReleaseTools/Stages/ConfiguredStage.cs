using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;
using ValidationContext = Kavita.ReleaseTools.Models.ValidationContext;
using ValidationContext2 = System.ComponentModel.DataAnnotations.ValidationContext;

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

        var results = new List<ValidationResult>();
        var context = new ValidationContext2(configuration);
        Validator.TryValidateObject(configuration, context, results, true);

        var issues = new List<ValidationIssue>();

        issues.AddRange(results.Select(r => new ValidationIssue
        {
            StageName = Name,
            Message = r.ErrorMessage ?? "Unknown validation error.",
            ExtraInfo = r.MemberNames.Any()
                ? string.Join(", ", r.MemberNames)
                : string.Empty,
            Solutions = []
        }));


        return [.. issues, .. Validate(ctx, configuration)];
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
