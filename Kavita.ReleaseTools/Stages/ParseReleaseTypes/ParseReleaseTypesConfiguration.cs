using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.ParseReleaseTypes;

public class ParseReleaseTypesConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Requierments per relase
    /// </summary>
    [Required]
    public required Dictionary<ReleaseType, BranchRequirement> BranchRequirements { get; init; }
}

public class BranchRequirement
{
    /// <summary>
    /// The branch has to start with
    /// </summary>
    public required string StartWiths { get; init; } = string.Empty;
    /// <summary>
    /// The branch cannot contain
    /// </summary>
    public required List<string> DoesNotContain { get; init; } = [];

    public bool IsMatch(string branch) => branch.StartsWith(StartWiths) && !DoesNotContain.Any(branch.Contains);
}
