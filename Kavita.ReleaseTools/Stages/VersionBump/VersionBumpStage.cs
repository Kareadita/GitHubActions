using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Commands.Git;
using Kavita.ReleaseTools.Models;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;
using Version = System.Version;

namespace Kavita.ReleaseTools.Stages.VersionBump;

public class VersionBumpStage(ILogger<VersionBumpStage> logger) : ConfiguredStage<VersionBumpConfiguration>(logger)
{

    public override string Name => nameof(VersionBumpStage);

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, VersionBumpConfiguration config)
    {
        return ValidationHelpers.ValidateCsprojPath(ctx, Name, ctx.Configuration.CsprojPath);
    }

    protected override VersionBumpConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.VersionBump;
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, VersionBumpConfiguration config, CancellationToken ct)
    {
        var newVersion = BumpVersion(config, ctx.ReleaseVersion);

        logger.LogInformation("Updating AssemblyVersion from {OldVersion} to {newVersion}", ctx.ReleaseVersion, newVersion);

        await SetAssemblyVersion(ctx, newVersion, ct);

        if (config.Commit)
        {
            var commitCommand = new GitCommitCommand.Builder()
                .WithCommitMessage(config.CommitMessage ?? "Bump Version")
                .WithCommitOptions(new CommitOptions { AllowEmptyCommit = false })
                .WithFile(ctx.Configuration.CsprojPath)
                .Build();
            await commitCommand.RunAsync(ctx, ct);
        }

        ctx.ReleaseVersion = newVersion;
    }

    private static async Task SetAssemblyVersion(ExecutionContext ctx, Version version, CancellationToken ct)
    {
        var match = ctx.VersionParseArtifacts.AssemblyMatch;

        var newContent = string.Concat(
            ctx.VersionParseArtifacts.AssemblyContent[..match.Index],
            match.Groups["openTag"].Value,
            version.ToString(),
            match.Groups["closeTag"].Value,
            ctx.VersionParseArtifacts.AssemblyContent[(match.Index + match.Length)..]);

        await ctx.FileSystem.File.WriteAllTextAsync(ctx.Configuration.CsprojPath, newContent, ct);
    }



    public static Version BumpVersion(VersionBumpConfiguration config, Version version)
    {
        var components = new[] { version.Major, version.Minor, version.Build, version.Revision };

        var count = version.Revision >= 0 ? 4
            : version.Build    >= 0 ? 3
            : 2;

        var index = (int) config.ComponentToBump;

        // Ensure all required fields are set (not -1)
        for (var i = count; i <= index; i++)
            components[i] = 0;
        count = Math.Max(count, index + 1);

        components[index]++;

        if (config.ResetSmallerComponents)
            for (var i = index + 1; i < components.Length; i++)
                components[i] = 0;

        return count switch
        {
            4 => new Version(components[0], components[1], components[2], components[3]),
            3 => new Version(components[0], components[1], components[2]),
            _ => new Version(components[0], components[1]),
        };
    }


}
