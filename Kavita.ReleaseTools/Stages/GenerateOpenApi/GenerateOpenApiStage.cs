using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.GenerateOpenApi;

public class GenerateOpenApiStage(ILogger<GenerateOpenApiStage> logger): ConfiguredStage<GenerateOpenApiConfiguration>(logger)
{
    public override string Name => nameof(GenerateOpenApiStage);

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, GenerateOpenApiConfiguration config)
    {
        return ValidationHelpers.ValidateCsprojPath(ctx, Name, config.CsprojPath);
    }

    protected override GenerateOpenApiConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.GenerateOpenApi;
    }

    protected override Task ExecuteAsync(ExecutionContext ctx, GenerateOpenApiConfiguration config, CancellationToken ct)
    {
        throw new System.NotImplementedException();
    }
}
