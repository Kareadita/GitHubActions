using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Commands.Git;
using Kavita.ReleaseTools.Models;
using LibGit2Sharp;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;
using Version = System.Version;

namespace Kavita.ReleaseTools.Stages.VersionBump;

public partial class VersionBumpStage : ConfiguredStage<VersionBumpConfiguration>
{
    private static readonly Regex AssemblyVersionPattern = AssemblyVersionRegex();

    public override string Name => nameof(VersionBumpStage);

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, VersionBumpConfiguration config)
    {
        return ValidationHelpers.ValidateCsprojPath(ctx, Name, config.CsprojPath);
    }

    protected override VersionBumpConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.VersionBump;
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, VersionBumpConfiguration config, CancellationToken ct)
    {
        var content = await ctx.FileSystem.File.ReadAllTextAsync(config.CsprojPath, ct);

        var currentVersion = ReadAssemblyVersion(content);
        if (currentVersion is null)
        {
            throw new ExecutionException($"Version could not be parsed from {config.CsprojPath}");
        }

        var newVersion = BumpVersion(config, currentVersion);

        // TODO: Log version bump

        var updated = SetAssemblyVersion(content, newVersion);
        await ctx.FileSystem.File.WriteAllTextAsync(config.CsprojPath, updated, ct);


        if (config.Commit)
        {
            var commitCommand = new GitCommitCommand.Builder()
                .WithCommitMessage(config.CommitMessage ?? "Bump Version")
                .WithCommitOptions(new CommitOptions { AllowEmptyCommit = false })
                .WithFile(config.CsprojPath)
                .Build();
            await commitCommand.RunAsync(ctx, ct);
        }

        ctx.ReleaseVersion = newVersion;
    }

    private static Version? ReadAssemblyVersion(string content)
    {
        var match = FindAssemblyVersion(content);
        return match is not null && Version.TryParse(match.Groups["version"].Value.Trim(), out var version)
            ? version
            : null;
    }

    private static string SetAssemblyVersion(string content, Version version)
    {
        var match = FindAssemblyVersion(content)
            ?? throw new ExecutionException("No AssemblyVersion element to write");

        return string.Concat(
            content[..match.Index],
            match.Groups["openTag"].Value,
            version.ToString(),
            match.Groups["closeTag"].Value,
            content[(match.Index + match.Length)..]);
    }

    private static Match? FindAssemblyVersion(string content)
    {
        var matches = AssemblyVersionPattern.Matches(content);
        if (matches.Count > 1)
        {
            throw new ExecutionException($"Found {matches.Count} AssemblyVersion elements, expected one");
        }

        return matches.Count == 1 ? matches[0] : null;
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

    [GeneratedRegex(@"(?<openTag><AssemblyVersion>)(?<version>\s*[^<]*?\s*)(?<closeTag></AssemblyVersion>)", RegexOptions.Compiled)]
    private static partial Regex AssemblyVersionRegex();
}
