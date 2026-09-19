using System.Collections.Generic;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages;

public static class ValidationHelpers
{
    /// <param name="propertyName">Config key to report, when it isn't <c>csprojPath</c></param>
    public static List<ValidationIssue> ValidateCsprojPath(ValidationContext ctx, string stageName, string? csprojPath,
        string propertyName = "csprojPath")
    {
        if (!ctx.FileSystem.File.Exists(csprojPath))
        {
            return [new ValidationIssue
            {
                StageName = stageName,
                Message = $"{propertyName} not found: {csprojPath}",
            }];
        }

        return [];
    }
}
