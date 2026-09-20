using System.ComponentModel.DataAnnotations;
using System.Linq;
using Kavita.ReleaseTools.Configuration;

namespace Kavita.ReleaseTools.Stages;

public class GitHubPrConfiguration
{
    [Required(ErrorMessage = "GITHUB_REPOSITORY must be set")]
    [FromEnvironment("GITHUB_REPOSITORY")]
    public string Repository { get; init; } = string.Empty;

    [Required(ErrorMessage = "PR_NUMBER must be set")]
    [FromEnvironment("PR_NUMBER")]
    public string PrNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "PR_TITLE must be set")]
    [FromEnvironment("PR_TITLE")]
    public string PrTitle { get; init; } = string.Empty;

    [Required(ErrorMessage = "PR_DESCRIPTION must be set")]
    [FromEnvironment("PR_DESCRIPTION")]
    public string PrDescription { get; init; } = string.Empty;

    public string Owner => Repository.Split('/').ElementAtOrDefault(0) ?? string.Empty;
    public string RepositoryName => Repository.Split('/').ElementAtOrDefault(1) ?? string.Empty;
}
