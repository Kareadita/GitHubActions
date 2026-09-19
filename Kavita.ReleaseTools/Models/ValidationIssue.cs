using System.Collections.Generic;

namespace Kavita.ReleaseTools.Models;

public class ValidationIssue
{
    public required string StageName { get; set; }
    public required string Message { get; set; }
    public string ExtraInfo { get; set; } = string.Empty;
    public List<string> Solutions { get; set; } = [];
}
