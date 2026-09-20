using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Configuration;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.FlushGitChanges;

/// <summary>
/// A stage that allows any changes to be commit in one go.
/// </summary>
/// <remarks>Disable commits on other stages to avoid extra's</remarks>
public class FlushGitChangesConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Branch to commit the changes to
    /// </summary>
    public string Branch { get; init; } = "main";

    /// <summary>
    /// A custom commit message
    /// </summary>
    public string? CommitMessage { get; init; }

    /// <summary>
    /// Which files should be tracked
    /// </summary>
    public required List<string> Files { get; init; }

    [Required]
    [FromEnvironment("GITHUB_TOKEN")]
    public string AuthToken { get; init; } = string.Empty;
}
