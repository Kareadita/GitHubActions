using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Stages.VersionBump;

public class VersionBumpConfiguration: IStageConfiguration
{
    public required bool Disable { get; set; }

    public required VersionComponent ComponentToBump { get; set; }

    public bool ResetSmallerComponents { get; set; }

    public required string CsprojPath { get; set; }

    public string? CommitMessage { get; set; }
}

public enum VersionComponent
{
    Major = 0,
    Minor = 1,
    Build = 2,
    Revision = 3,
}
