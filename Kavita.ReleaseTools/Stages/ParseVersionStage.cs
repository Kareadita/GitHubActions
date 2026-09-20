using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages;

/// <summary>
/// This stage always runs, and cannot be configured
/// </summary>
public partial class ParseVersionStage(ILogger<ParseVersionStage> logger): IStage
{
    private static readonly Regex AssemblyVersionPattern = AssemblyVersionRegex();

    public string Name => nameof(ParseVersionStage);
    public IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx)
    {
        return ValidationHelpers.ValidateCsprojPath(ctx, Name, ctx.Configuration.CsprojPath);
    }

    public async Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        var content = await ctx.FileSystem.File.ReadAllTextAsync(ctx.Configuration.CsprojPath, ct);
        var version = FindAssemblyVersion(content);
        if (version is null)
        {
            throw new ExecutionException($"Could not find AssemblyVersion element, {ctx.Configuration.CsprojPath}");
        }

        if (!Version.TryParse(version.Groups["version"].Value.Trim(), out var currentVersion))
        {
            throw new ExecutionException($"Could not parse AssemblyVersion element, {ctx.Configuration.CsprojPath}");
        }

        ctx.ReleaseVersion = currentVersion;
        ctx.VersionParseArtifacts = new VersionParseArtifacts
        {
            AssemblyMatch = version,
            AssemblyContent = content,
        };

        logger.LogInformation("Parsed AssemblyVersion: {Version}", version);
    }

    [GeneratedRegex(@"(?<openTag><AssemblyVersion>)(?<version>\s*[^<]*?\s*)(?<closeTag></AssemblyVersion>)", RegexOptions.Compiled)]
    private static partial Regex AssemblyVersionRegex();

    private static Match? FindAssemblyVersion(string content)
    {
        var matches = AssemblyVersionPattern.Matches(content);
        if (matches.Count > 1)
        {
            throw new ExecutionException($"Found {matches.Count} AssemblyVersion elements, expected one");
        }

        return matches.Count == 1 ? matches[0] : null;
    }
}
