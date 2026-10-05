using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Models;
using Kavita.ReleaseTools.Stages.VersionBump;
using Microsoft.Extensions.Logging;
using Octokit;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.GitHubComments;

public partial class GitHubCommentsStage(ILogger<GitHubCommentsStage> logger) : ConfiguredStage<GitHubCommentsConfiguration>(logger)
{

    [GeneratedRegex(@"(?:close[sd]?|fix(?:e[sd])?|resolve[sd]?)\s+#(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex LinkedIssuePattern { get; }

    private const string DefaultBotLogin = "github-actions[bot]";

    public override string Name => nameof(GitHubCommentsStage);

    protected override GitHubCommentsConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.GitHubComments;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, GitHubCommentsConfiguration config)
    {
        return [];
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, GitHubCommentsConfiguration config, CancellationToken ct)
    {
        if (ctx.Configuration.DryRun)
        {
            logger.LogInformation("Skipping GitHub Comments in dry run");
            return;
        }

        var owner = config.Owner;
        var repo = config.Repository;
        var prNumber = config.PrNumber;
        var nightlyVersion = ctx.ReleaseVersion;
        var stableVersion = VersionBumpStage.BumpVersion(new VersionBumpConfiguration
        {
            Disabled = false,
            ComponentToBump = config.ReleaseComponent,
            ResetSmallerComponents = config.ResetSmallerComponents,
        }, ctx.ReleaseVersion);

        var client = new GitHubClient(new ProductHeaderValue("Kavita.ReleaseTools"))
            { Credentials = new Credentials(config.AuthToken) };

        var textToSearch = $"{config.PrTitle}\n{config.PrDescription}";

        var linkedIssues = LinkedIssuePattern.Matches(textToSearch)
            .Select(match => match.Groups[1].Value)
            .Select(number => int.TryParse(number, out var result) ?  result : 0)
            .ToList();

        if (linkedIssues.Count == 0)
        {
            logger.LogInformation("No linked issues found");
            return;
        }

        logger.LogInformation("Found {Count} linked issues: {Issues}", linkedIssues.Count, string.Join(',', linkedIssues));

        var commentBody =
            $"This was closed as of PR #{prNumber}, it is available in " +
            $"v{nightlyVersion} (nightly) and will be available in " +
            $"v{stableVersion} (stable).";

        int posted = 0, skipped = 0, failed = 0;

        foreach (var issue in linkedIssues)
        {
            try
            {
                var issueData = await client.Issue.Get(owner, repo, issue);

                if (issueData.PullRequest is not null)
                {
                    logger.LogInformation("#{IssueNumber}: skipped (pull request)", issue);
                    skipped++;
                    continue;
                }

                var comments = await client.Issue.Comment.GetAllForIssue(owner, repo, issue);

                var alreadyCommented = comments.Any(c =>
                    string.Equals(c.User?.Login, DefaultBotLogin, System.StringComparison.OrdinalIgnoreCase) &&
                    c.Body?.Contains($"PR #{prNumber}") == true);

                if (alreadyCommented)
                {
                    logger.LogInformation("#{IssueNumber}: skipped (already commented)", issue);
                    skipped++;
                    continue;
                }

                await client.Issue.Comment.Create(owner, repo, issue, commentBody);

                logger.LogInformation("#{IssueNumber}: commented", issue);
                posted++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "#{IssueNumber}: failed",  issue);

                failed++;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
        }


        logger.LogInformation("Summary: {Posted} posted, {Skipped} skipped, {Failed} failed", posted, skipped, failed);
    }
}
