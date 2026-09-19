using Kavita.ReleaseTools.Commands;
using Kavita.ReleaseTools.Stages.BuildFrontend;
using Kavita.ReleaseTools.Stages.FlushGitChanges;
using Kavita.ReleaseTools.Stages.GenerateOpenApi;
using Kavita.ReleaseTools.Stages.VersionBump;

namespace Kavita.ReleaseTools.Models;

public class ReleaseConfiguration
{

    /// <summary>
    /// A dry run won't execute any <see cref="MutatingCommand"/>
    /// </summary>
    public bool DryRun { get; init; } = false;
    /// <summary>
    /// Should disable stages be validated
    /// </summary>
    public bool ValidateDisabledStages { get; set; } = true;

    public VersionBumpConfiguration? VersionBump { get; init; }
    public GenerateOpenApiConfiguration? GenerateOpenApi { get; init; }
    public FlushGitChangesConfiguration? FlushGitChanges { get; init; }
    public BuildFrontendConfiguration? BuildFrontend { get; init; }

}
