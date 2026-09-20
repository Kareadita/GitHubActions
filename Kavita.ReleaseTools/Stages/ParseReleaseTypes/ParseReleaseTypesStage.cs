using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.ParseReleaseTypes;

public class ParseReleaseTypesStage(ILogger<ParseReleaseTypesStage> logger) : ConfiguredStage<ParseReleaseTypesConfiguration>(logger)
{
    public override string Name => nameof(ParseReleaseTypesStage);
    protected override ParseReleaseTypesConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.ParseReleaseTypes;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, ParseReleaseTypesConfiguration config)
    {
        return [];
    }

    protected override Task ExecuteAsync(ExecutionContext ctx, ParseReleaseTypesConfiguration config, CancellationToken ct)
    {
        List<ReleaseType> releaseTypes = [];

        foreach (var (releaseType, branchRequirement) in config.BranchRequirements)
        {
            if (branchRequirement.IsMatch(config.HeadRef))
                releaseTypes.Add(releaseType);
        }

        ctx.ReleaseTypes = releaseTypes;

        logger.LogInformation("Releases: {Releases}", string.Join(", ", releaseTypes.Select(rt => rt.ToString())));

        return Task.CompletedTask;
    }
}
