using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.BuildLibrary;

public class BuildLibraryStage(ILogger<BuildLibraryStage> logger, IProcessRunner runner) : ConfiguredStage<BuildLibraryConfiguration>(logger)
{
    private const string Dotnet = "dotnet";

    public override string Name => nameof(BuildLibraryStage);
    protected override BuildLibraryConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.BuildLibrary;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, BuildLibraryConfiguration config)
    {
        return ValidationHelpers.ValidateCsprojPath(ctx, Name, config.CsprojPath);
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, BuildLibraryConfiguration config, CancellationToken ct)
    {
        await new ProcessCommand.Builder(runner)
            .WithExecutable(Dotnet)
            .WithArguments("build", config.CsprojPath, "--configuration", config.Configuration)
            .Build()
            .RunAsync(ctx, ct);

        await new ProcessCommand.Builder(runner)
            .WithExecutable(Dotnet)
            .WithArguments("pack", config.CsprojPath, "--configuration", config.Configuration, "--output", config.OutputDirectory)
            .Build()
            .RunAsync(ctx, ct);
    }
}
