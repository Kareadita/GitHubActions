using System.Collections.Generic;
using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Stages.FlushGitChanges;

public class FlushGitChangesConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disable { get; init; }

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
    public List<string> Files { get; init; }
}
