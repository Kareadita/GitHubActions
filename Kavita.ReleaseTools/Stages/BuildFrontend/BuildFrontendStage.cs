using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.BuildFrontend;

public class BuildFrontendStage(ILogger<BuildFrontendStage> logger, IProcessRunner runner) : ConfiguredStage<BuildFrontendConfiguration>(logger)
{
    private const string Npm = "npm";
    public override string Name => nameof(BuildFrontendStage);
    protected override BuildFrontendConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.BuildFrontend;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, BuildFrontendConfiguration config)
    {
        List<ValidationIssue> issues = [];

        if (!string.IsNullOrEmpty(config.Path) && !ctx.FileSystem.Directory.Exists(config.Path))
            issues.Add(Issue($"[{nameof(BuildFrontendConfiguration.Path)}] The path «{config.Path}» cannot be found"));

        return issues;
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, BuildFrontendConfiguration config, CancellationToken ct)
    {
        await new ProcessCommand.Builder(runner)
            .WithExecutable(Npm)
            .WithArguments("ci")
            .AppendArgumentIf(config.AllowLegacyPeerDeps, "--legacy-peer-deps")
            .WithWorkingDirectory(config.Path)
            .Build()
            .RunAsync(ctx, ct);

        await new ProcessCommand.Builder(runner)
            .WithExecutable(Npm)
            .WithArguments("run", config.BuildScript)
            .WithWorkingDirectory(config.Path)
            .Build()
            .RunAsync(ctx, ct);

        if (string.IsNullOrEmpty(config.CopyTo))
            return;

        var destination = config.CopyTo;

        if (config.FullReplace && ctx.FileSystem.Directory.Exists(destination))
        {
            logger.LogDebug("FullReplace enabled, deleting {Destination}", destination);
            ctx.FileSystem.Directory.Delete(destination, recursive: true);
        }

        var outputPath = ctx.FileSystem.Path.Combine(config.Path, config.OutputPath);

        if (!ctx.FileSystem.Directory.Exists(destination))
        {
            logger.LogDebug("Moving {Source} to {Destination}", outputPath, destination);
            ctx.FileSystem.Directory.Move(outputPath, destination);
            return;
        }

        logger.LogDebug("Merging {Source} into {Destination}", outputPath, destination);
        CopyDirectory(ctx, outputPath, destination);
        ctx.FileSystem.Directory.Delete(outputPath, recursive: true);
    }

    private static void CopyDirectory(ExecutionContext ctx, string source, string destination)
    {
        ctx.FileSystem.Directory.CreateDirectory(destination);

        foreach (var file in ctx.FileSystem.Directory.GetFiles(source))
        {
            var fileName = ctx.FileSystem.Path.GetFileName(file);
            var target = ctx.FileSystem.Path.Combine(destination, fileName);
            ctx.FileSystem.File.Copy(file, target, overwrite: true);
        }

        foreach (var dir in ctx.FileSystem.Directory.GetDirectories(source))
        {
            var dirName = ctx.FileSystem.Path.GetFileName(dir);
            var target = ctx.FileSystem.Path.Combine(destination, dirName);
            if (ctx.FileSystem.Directory.Exists(target))
            {
                CopyDirectory(ctx, dir, target);
            }
            else
            {
                ctx.FileSystem.Directory.Move(dir, target);
            }
        }
    }
}
