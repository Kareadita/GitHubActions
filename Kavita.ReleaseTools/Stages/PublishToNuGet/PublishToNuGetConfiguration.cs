using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.PublishToNuGet;

public class PublishToNuGetConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    [Required]
    [FromEnvironment("NUGET_API_KEY")]
    public string ApiKey { get; init; } = string.Empty;

    [Required]
    public required string OutputDirectory { get; init; } = "./artifacts";

    [Required]
    public required string Source { get; init; } = "https://api.nuget.org/v3/index.json";

    public bool SkipDuplicate { get; init; } = true;
}
