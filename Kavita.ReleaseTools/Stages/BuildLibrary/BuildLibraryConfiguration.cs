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

    [Required(ErrorMessage = "CsprojPath is required")]
    public required string CsprojPath { get; init; }

    [Required]
    public required string Configuration { get; init; } = "Release";

    [Required]
    public required string OutputDirectory { get; init; } = "./artifacts";
}
