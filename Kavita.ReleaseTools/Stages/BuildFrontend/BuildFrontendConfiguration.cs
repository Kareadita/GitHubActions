using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Stages.BuildFrontend;

public class BuildFrontendConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    public required string Path { get; init; }
    public required string BuildScript { get; init; }
    public required string OutputPath { get; init; }
    public required string CopyTo { get; init; }
    /// <summary>
    /// When true, deletes the destination directory before copying.
    /// When false (default), merges the output into the destination,
    /// overwriting files that already exist.
    /// </summary>
    public bool FullReplace { get; set; } = false;
}
