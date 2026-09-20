using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Configuration;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.CreateGitHubRelease;

public class CreateGitHubReleaseConfiguration: GitHubPrConfiguration, IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Path to assets to upload for the release
    /// </summary>
    [Required]
    public required string AssetPath { get; init; }

    /// <summary>
    /// Release tag template. Use {Version}
    /// </summary>
    [Required]
    public required string VersionTemplate { get; init; } = "v{Version}";

    [Required]
    public required string TitleTemplate { get; init; } = "v{Version} - {PrTitle}";

    [Required(ErrorMessage = "GITHUB_TOKEN must be set")]
    [FromEnvironment("GITHUB_TOKEN")]
    public string AuthToken { get; init; } = string.Empty;
}
