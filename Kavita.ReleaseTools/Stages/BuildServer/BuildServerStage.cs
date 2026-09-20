using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.BuildServer;

public class BuildServerStage(ILogger<BuildServerStage> logger, IProcessRunner runner) : ConfiguredStage<BuildServerConfiguration>(logger)
{
    private const string Dotnet = "dotnet";
    private const string Tar = "tar";
    public override string Name => nameof(BuildServerStage);

    protected override BuildServerConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.BuildServer;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, BuildServerConfiguration config)
    {
        List<ValidationIssue> issues = [];

        if (!ctx.FileSystem.File.Exists(config.SlnPath))
            issues.Add(Issue($"SlnPath does not found on disk ({config.SlnPath})"));

        if (!ctx.FileSystem.File.Exists(config.CsprojPath))
            issues.Add(Issue($"CsprojPath does not found on disk ({config.CsprojPath})"));

        if (!config.OutputPath.EndsWith('/'))
            issues.Add(Issue($"OutputPath does not end with /"));

        return issues;
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, BuildServerConfiguration config, CancellationToken ct)
    {
        var buildPath = ctx.FileSystem.Path.Combine(ctx.FileSystem.Path.GetTempPath(), $"{config.AppName}-release-{Guid.NewGuid():N}");

        var rids = config.Rids
            .Where(kv => ctx.ReleaseTypes.Contains(kv.Key))
            .SelectMany(kv => kv.Value)
            .Distinct();

        foreach (var rid in rids)
        {
            await PackageRid(ctx, config, buildPath, rid, ct);
        }
    }

    private async Task PackageRid(ExecutionContext ctx, BuildServerConfiguration config, string buildPath, string rid, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("Starting build process for {Rid}", rid);

        var outputPath = ctx.FileSystem.Path.Combine(buildPath, rid, config.AppName);

        await new ProcessCommand.Builder(runner)
            .WithExecutable(Dotnet)
            .WithArguments("msbuild", "-restore", config.SlnPath, $"-p:Configuration={config.Configuration}", "-p:Platform=\"Any CPU\"", $"-p:RuntimeIdentifiers={rid}")
            .Build()
            .RunAsync(ctx, ct);

        await new ProcessCommand.Builder(runner)
            .WithExecutable(Dotnet)
            .WithArguments("publish", config.CsprojPath, "-c", config.Configuration, "--no-restore", "--runtime", rid, "-o", outputPath)
            .AppendArgumentIf(config.SelfContained, "--self-contained")
            .Build()
            .RunAsync(ctx, ct);

        foreach (var path in config.DeletePaths)
        {
            var completePath = ctx.FileSystem.Path.Combine(outputPath, path);

            logger.LogTrace("Deleting {Path}", completePath);
            if (ctx.FileSystem.Directory.Exists(completePath))
            {
                ctx.FileSystem.Directory.Delete(completePath, true);
            }
            else if (ctx.FileSystem.File.Exists(completePath))
            {
                ctx.FileSystem.File.Delete(completePath);
            }
        }

        foreach (var fromTo in config.CopyFiles)
        {
            var destPath = ctx.FileSystem.Path.Combine(outputPath, fromTo.To);
            if (ctx.FileSystem.File.Exists(fromTo.From))
            {
                var directory = ctx.FileSystem.Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(directory) && !ctx.FileSystem.Directory.Exists(directory))
                    ctx.FileSystem.Directory.CreateDirectory(directory);

                logger.LogTrace("Copying {From} to {To}", fromTo.To, destPath);
                if (ctx.FileSystem.File.Exists(destPath))
                    ctx.FileSystem.File.Delete(destPath);

                ctx.FileSystem.File.Copy(fromTo.From, destPath);
            }
            else
            {
                logger.LogWarning("Cannot find configured file at path {Path} to copy, skipping", fromTo.From);
            }
        }

        foreach (var fromTo in config.RenameFiles)
        {
            var srcPath = ctx.FileSystem.Path.Combine(outputPath, fromTo.From);
            var destPath = ctx.FileSystem.Path.Combine(outputPath, fromTo.To);
            if (ctx.FileSystem.File.Exists(srcPath))
            {
                logger.LogTrace("Renaming {From} to {To}", srcPath, destPath);
                ctx.FileSystem.File.Move(srcPath, destPath);
            }
            else
            {
                logger.LogWarning("Cannot find configured file at path {Path} to rename", srcPath);
            }
        }

        logger.LogTrace("Compressing {Path}", outputPath);

        if (!ctx.FileSystem.Directory.Exists(config.OutputPath))
            ctx.FileSystem.Directory.CreateDirectory(config.OutputPath);

        var parent = ctx.FileSystem.Path.GetDirectoryName(outputPath)!;
        var archiveName = $"{config.AppName.ToLowerInvariant()}-{rid}.tar.gz";

        await new ProcessCommand.Builder(runner)
            .WithExecutable(Tar)
            .WithArguments("-czvf", $"{config.OutputPath}{archiveName}", "-C", parent, config.AppName)
            .Build()
            .RunAsync(ctx, ct);

        sw.Stop();
        logger.LogInformation("Completed build process for {Rid} in {TotalSeconds}s", rid, sw.Elapsed.TotalSeconds);


    }
}
