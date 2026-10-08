using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Configuration;
using Kavita.ReleaseTools.Models;
using Kavita.ReleaseTools.Stages.VersionBump;

namespace Kavita.ReleaseTools.Stages.GitHubComments;

public class GitHubCommentsConfiguration: GitHubPrConfiguration, IStageConfiguration
{
    public bool Disabled { get; init; }
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    [EnumDataType(typeof(VersionComponent))]
    [Required(ErrorMessage = "ReleaseComponent is required")]
    public required VersionComponent ReleaseComponent { get; init; }

    public bool ResetSmallerComponents { get; init; } = true;

    [Required(ErrorMessage = "GITHUB_TOKEN must be set")]
    [FromEnvironment("GITHUB_TOKEN")]
    public string AuthToken { get; init; } = string.Empty;
}
