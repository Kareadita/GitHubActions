using System.Collections.Generic;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages;

public static class ValidationHelpers
{
    public static List<ValidationIssue> ValidateCsprojPath(ValidationContext ctx, string stageName, string csprojPath)
    {
        if (string.IsNullOrWhiteSpace(csprojPath))
        {
            return [new ValidationIssue
                {
                    StageName = stageName,
                    Message = $"{nameof(csprojPath)} is not configured",
                }
            ];
        }

        if (!ctx.FileSystem.File.Exists(csprojPath))
        {
            return [new ValidationIssue
            {
                StageName = stageName,
                Message = $"{nameof(csprojPath)} not found: {csprojPath}",
            }];
        }

        return [];
    }
}
