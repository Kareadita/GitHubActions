using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Json.Schema.Generation.Serialization;
using Kavita.ReleaseTools.Commands;
using Kavita.ReleaseTools.Stages.BuildFrontend;
using Kavita.ReleaseTools.Stages.BuildLibrary;
using Kavita.ReleaseTools.Stages.BuildServer;
using Kavita.ReleaseTools.Stages.Docker;
using Kavita.ReleaseTools.Stages.FlushGitChanges;
using Kavita.ReleaseTools.Stages.GenerateOpenApi;
using Kavita.ReleaseTools.Stages.NotifyDiscord;
using Kavita.ReleaseTools.Stages.ParseReleaseTypes;
using Kavita.ReleaseTools.Stages.PublishToNuGet;
using Kavita.ReleaseTools.Stages.RunScript;
using Kavita.ReleaseTools.Stages.VersionBump;

namespace Kavita.ReleaseTools.Models;

[GenerateJsonSchema]
public class ReleaseConfiguration: IValidatableObject
{

    /// <summary>
    /// A dry run won't execute any <see cref="MutatingCommand"/>
    /// </summary>
    public bool DryRun { get; init; } = false;
    /// <summary>
    /// Should disable stages be validated
    /// </summary>
    public bool ValidateDisabledStages { get; set; } = true;

    [Required]
    public required GitData GitData { get; init; } = new();

    public VersionBumpConfiguration? VersionBump { get; init; }
    public GenerateOpenApiConfiguration? GenerateOpenApi { get; init; }
    public FlushGitChangesConfiguration? FlushGitChanges { get; init; }
    public BuildFrontendConfiguration? BuildFrontend { get; init; }
    public BuildServerConfiguration? BuildServer { get; init; }
    public BuildLibraryConfiguration? BuildLibrary { get; init; }
    public PublishToNuGetConfiguration? PublishToNuGet { get; init; }
    public NotifyDiscordConfiguration? NotifyDiscord { get; init; }
    public DockerConfiguration? Docker { get; init; }
    public RunScriptConfiguration? RunScript { get; init; }

    public ParseReleaseTypesConfiguration? ParseReleaseTypes { get; init; } = new()
    {
        BranchRequirements = new Dictionary<ReleaseType, BranchRequirement>
        {
            // Stable releases come from release/
            [ReleaseType.Stable] = new() {DoesNotContain = [], StartWiths = "release/"},
            // Nightly is not stable or Canary
            [ReleaseType.Nightly] = new() {DoesNotContain = ["canary/", "release/"], StartWiths = ""},
            // Canary releases come from canary/
            [ReleaseType.Canary] = new() {DoesNotContain = [], StartWiths = "canary/"},
        }
    };

    public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
    {
        System.ComponentModel.DataAnnotations.ValidationContext ctx = new(GitData);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(GitData, ctx, results, validateAllProperties: true);
        return results.Select(r => new ValidationResult(r.ErrorMessage, r.MemberNames.Select(m => $"GitData.{m}")));
    }
}
