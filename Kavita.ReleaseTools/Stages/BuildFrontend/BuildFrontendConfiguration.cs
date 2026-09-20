using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.BuildFrontend;

public class BuildFrontendConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Working directory. (Where package.json is located)
    /// </summary>
    [Required(ErrorMessage = "Path is required")]
    public required string Path { get; init; }

    /// <summary>
    /// Which script to run to build the frontend
    /// </summary>
    [Required(ErrorMessage = "BuildScript is required")]
    public required string BuildScript { get; init; }

    /// <summary>
    /// Path relative to <see cref="Path"/> where the frontend is created
    /// </summary>
    [Required(ErrorMessage = "OutputPath is required")]
    public required string OutputPath { get; init; }

    /// <summary>
    /// Where should the artefacts be copied to, requires <see cref="OutputPath"/>
    /// </summary>
    public required string CopyTo { get; init; }
    /// <summary>
    /// When true, deletes the destination directory before copying.
    /// When false (default), merges the output into the destination,
    /// overwriting files that already exist.
    /// </summary>
    public bool FullReplace { get; set; } = false;

    /// <summary>
    /// Should legacy peer dep be enabled when installing dependencies
    /// </summary>
    public bool AllowLegacyPeerDeps { get; init; } = false;
}
