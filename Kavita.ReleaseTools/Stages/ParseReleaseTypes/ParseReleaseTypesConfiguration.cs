using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.ParseReleaseTypes;

public class ParseReleaseTypesConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    [Required]
    public required Dictionary<ReleaseType, BranchRequirement> BranchRequirements { get; init; }
}

public class BranchRequirement
{

    public required string StartWiths { get; init; } = string.Empty;
    public required string DoesNotContain { get; init; } = string.Empty;

    public bool IsMatch(string branch) => branch.StartsWith(StartWiths) && !branch.Contains(DoesNotContain);
}
