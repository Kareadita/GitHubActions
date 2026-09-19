using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Stages.VersionBump;

/// <summary>
/// A stage that allows you to bump the AssemblyVersion in a .csproj file
/// </summary>
public class VersionBumpConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public required bool Disabled { get; init; }

    /// <summary>
    /// Should this stage create a seperate commit
    /// </summary>
    public bool Commit { get; init; } = false;

    /// <summary>
    /// Which part of the version should be bumped
    /// </summary>
    public required VersionComponent ComponentToBump { get; init; }

    /// <summary>
    /// Should parts of the version smaller than <see cref="ComponentToBump"/> be reinit to 0
    /// </summary>
    public bool ResetSmallerComponents { get; init; }

    /// <summary>
    /// Path the to .csproj file that contains the version to bump
    /// </summary>
    public required string CsprojPath { get; init; }

    /// <summary>
    /// Custom commit message. Only relevant if <see cref="Commit"/> is true
    /// </summary>
    public string? CommitMessage { get; init; }
}

public enum VersionComponent
{
    Major = 0,
    Minor = 1,
    Build = 2,
    Revision = 3,
}
