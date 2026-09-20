using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Configuration;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.PublishToNuGet;

public class PublishToNuGetConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Your NuGet ApiKey. Do not store this in Git. Prefer the NUGET_API_KEY env var
    /// </summary>
    [Required]
    [FromEnvironment("NUGET_API_KEY")]
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Where our the nupkg files located
    /// </summary>
    [Required]
    public required string OutputDirectory { get; init; } = "./artifacts";

    /// <summary>
    /// NuGet source
    /// </summary>
    [Required]
    public required string Source { get; init; } = "https://api.nuget.org/v3/index.json";

    /// <summary>
    /// Skip duplicate version, defaults to true
    /// </summary>
    public bool SkipDuplicate { get; init; } = true;
}
