using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataAnnotationsContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace Kavita.ReleaseTools.Models;

/// <summary>
/// Turns DataAnnotations results into <see cref="ValidationIssue"/>s, so the root configuration and
/// every stage report their problems the same way.
/// </summary>
public static class ValidationIssueFactory
{
    public static List<ValidationIssue> FromAnnotations(string stageName, object target)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(target, new DataAnnotationsContext(target), results,
            validateAllProperties: true);

        return [.. results.Select(result => new ValidationIssue
        {
            StageName = stageName,
            Message = result.ErrorMessage ?? "Unknown validation error.",
            ExtraInfo = result.MemberNames.Any()
                ? string.Join(", ", result.MemberNames)
                : string.Empty,
            Solutions = []
        })];
    }
}
