using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace Kavita.ReleaseTools.Stages.BuildServer;

public class BuildServerConfiguration: IStageConfiguration, IValidatableObject
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Your application name (used for tar files)
    /// </summary>
    [Required]
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// Path to the solution file
    /// </summary>
    [Required(ErrorMessage = "SlnPath is required")]
    public required string SlnPath { get; init; }

    /// <summary>
    /// Path to the project to build
    /// </summary>
    [Required(ErrorMessage = "CsprojPath is required")]
    public required string CsprojPath { get; init; }

    /// <summary>
    /// For which Runtime Identifiers should be packaged
    /// </summary>
    /// <remarks>https://learn.microsoft.com/en-us/dotnet/core/rid-catalog</remarks>
    [Required(ErrorMessage = "Rids is required")]
    public required List<string> Rids { get; init; } = [];

    /// <summary>
    /// Which configuration to build against; Defaults to Release
    /// </summary>
    public string Configuration { get; init; } = "Release";

    /// <summary>
    /// Build self contained
    /// </summary>
    public bool SelfContained { get; init; } = true;

    /// <summary>
    /// Where should the tar balls be stored
    /// </summary>
    public string OutputPath { get; init; } = "./";

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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        for (var i = 0; i < RenameFiles.Count; i++)
        {
            foreach (var r in ValidateNested(RenameFiles[i], $"{nameof(RenameFiles)}[{i}]"))
                yield return r;
        }

        for (var i = 0; i < CopyFiles.Count; i++)
        {
            foreach (var r in ValidateNested(CopyFiles[i], $"{nameof(CopyFiles)}[{i}]"))
                yield return r;
        }
    }

    private static IEnumerable<ValidationResult> ValidateNested(object obj, string prefix)
    {
        var ctx = new ValidationContext(obj);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(obj, ctx, results, validateAllProperties: true);

        foreach (var r in results)
        {
            yield return new ValidationResult(
                r.ErrorMessage,
                r.MemberNames.Select(m => $"{prefix}.{m}").DefaultIfEmpty(prefix));
        }
    }
}

public class FromTo
{
    [Required]
    public required string From { get; init; }
    [Required]
    public required string To { get; init; }
}
