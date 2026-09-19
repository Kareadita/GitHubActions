using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages;

public abstract class ConfiguredStage<TConfiguration>: IStage
where TConfiguration : IStageConfiguration
{
    public abstract string Name { get; }

    public abstract IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx);

    public async Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        var configuration = GetConfiguration(ctx);
        if (configuration is null || configuration.Disable)
        {
            // TODO: Log for verbose mode that a stage was skipped
            return;
        }

        await ExecuteAsync(ctx, configuration, ct);
    }

    protected abstract TConfiguration? GetConfiguration(ExecutionContext ctx);

    protected abstract Task ExecuteAsync(ExecutionContext ctx, TConfiguration config, CancellationToken ct);


}
