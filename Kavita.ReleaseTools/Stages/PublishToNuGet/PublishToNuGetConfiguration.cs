using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Stages.PublishToNuGet;

public class PublishToNuGetConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    [Required]
    [FromEnvironment("NUGET_API_KEY")]
    public string ApiKey { get; init; } = string.Empty;

    [Required]
    public required string OutputDirectory { get; init; } = "./artifacts";

    [Required]
    public required string Source { get; init; } = "https://api.nuget.org/v3/index.json";

    public bool SkipDuplicate { get; init; } = true;
}
