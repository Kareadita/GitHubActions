using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.VersionBump;

/// <summary>
/// A stage that allows you to bump the AssemblyVersion in a .csproj file
/// </summary>
public class VersionBumpConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public required bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Should this stage create a seperate commit
    /// </summary>
    public bool Commit { get; init; } = false;

    /// <summary>
    /// Which part of the version should be bumped
    /// </summary>
    [EnumDataType(typeof(VersionComponent))]
    [Required(ErrorMessage = "ComponentToBump is required")]
    public required VersionComponent ComponentToBump { get; init; }  = (VersionComponent)(-1);

    /// <summary>
    /// Should parts of the version smaller than <see cref="ComponentToBump"/> be reinit to 0
    /// </summary>
    public bool ResetSmallerComponents { get; init; }

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
