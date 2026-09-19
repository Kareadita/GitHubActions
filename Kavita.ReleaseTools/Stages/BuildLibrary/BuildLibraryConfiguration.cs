using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Stages.BuildLibrary;

public class BuildLibraryConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    [Required(ErrorMessage = "CsprojPath is required")]
    public required string CsprojPath { get; init; }

    [Required]
    public required string Configuration { get; init; } = "Release";

    [Required]
    public required string OutputDirectory { get; init; } = "./artifacts";
}
