using System.ComponentModel.DataAnnotations;

namespace Kavita.ReleaseTools.Models;

/// <summary>
/// Data passed down from GitHub Actions. This is all assumed to be set in env. Unprocessed
/// </summary>
public class GitData: IHasEnvironmentValues
{
    [Required]
    [FromEnvironment("GITHUB_TOKEN")]
    public string AuthToken { get; init; } = string.Empty;

    [Required]
    [FromEnvironment("GITHUB_REPOSITORY")]
    public string Repository { get; init; } = string.Empty;

    [Required]
    [FromEnvironment("PR_NUMBER")]
    public string PrNumber { get; init; } = string.Empty;

    [Required]
    [FromEnvironment("PR_TITLE")]
    public string PrTitle { get; init; } = string.Empty;

    [Required]
    [FromEnvironment("PR_DESCRIPTION")]
    public string PrDescription { get; init; } = string.Empty;
}
