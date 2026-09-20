using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.BuildLibrary;

public class BuildLibraryConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Path to the project file
    /// </summary>
    [Required(ErrorMessage = "CsprojPath is required")]
    public required string CsprojPath { get; init; }

    /// <summary>
    /// Which configuration to build against, defaults to Release
    /// </summary>
    [Required]
    public required string Configuration { get; init; } = "Release";

    /// <summary>
    /// Where should the nupkg be generated to
    /// </summary>
    [Required]
    public required string OutputDirectory { get; init; } = "./artifacts";
}
