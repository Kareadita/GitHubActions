using Kavita.ReleaseTools.Stages.FlushGitChanges;
using Kavita.ReleaseTools.Stages.VersionBump;

namespace Kavita.ReleaseTools.Models;

public class ReleaseConfiguration
{
    public VersionBumpConfiguration? VersionBump { get; init; }
    public FlushGitChangesConfiguration? FlushGitChanges { get; init; }

}
