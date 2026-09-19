using System.Collections.Generic;
using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Stages.BuildServer;

public class BuildServerConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    public required string SlnPath { get; init; }
    public required string CsprojPath { get; init; }
    public required List<string> Rids { get; init; }
    public string Configuration { get; init; } = "Release";
    public bool SelfContained { get; init; } = true;
    /// <summary>
    /// Rename files inside the output path
    /// </summary>
    public List<FromTo> RenameFiles { get; init; } = [];
    /// <summary>
    /// Copy files from the git repository in the output paths (relative paths)
    /// </summary>
    public List<FromTo> CopyFiles { get; init; } = [];
    /// <summary>
    /// Deletes files in the output path
    /// </summary>
    public List<string> DeletePaths { get; init; } = [];
}

public class FromTo
{
    public required string From { get; init; }
    public required string To { get; init; }
}
