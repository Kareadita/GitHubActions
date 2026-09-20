using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using Octokit;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.CreateGitHubRelease;

public class CreateGitHubReleaseStage(ILogger<CreateGitHubReleaseStage> logger) : ConfiguredStage<CreateGitHubReleaseConfiguration>(logger)
{
    public override string Name => nameof(CreateGitHubReleaseStage);
    protected override CreateGitHubReleaseConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.CreateGitHubRelease;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, CreateGitHubReleaseConfiguration config)
    {
        return [];
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, CreateGitHubReleaseConfiguration config, CancellationToken ct)
    {
        var client = new GitHubClient(new ProductHeaderValue("Kavita.ReleaseTools"))
        { Credentials = new Credentials(config.AuthToken) };

        var newRelease = new NewRelease(config.VersionTemplate.Replace("{Version}", ctx.ReleaseVersion.ToString()))
        {
            Name = config.TitleTemplate
                .Replace("{Version}", ctx.ReleaseVersion.ToString())
                .Replace("{PrTitle}", config.PrTitle),
            Body = config.PrDescription,
            Draft = true,
        };

        var release = await client.Repository.Release.Create(config.Owner, config.RepositoryName, newRelease);
        logger.LogInformation("Created draft release {Name}", release.Name);

        var hasAssets = !string.IsNullOrEmpty(config.AssetPath)
                        && ctx.FileSystem.Directory.Exists(config.AssetPath);

        if (hasAssets)
        {
            try
            {
                foreach (var file in ctx.FileSystem.Directory.EnumerateFiles(config.AssetPath))
                {
                    await using var fs = ctx.FileSystem.File.OpenRead(file);
                    await client.Repository.Release.UploadAsset(release, new ReleaseAssetUpload
                    {
                        FileName = ctx.FileSystem.Path.GetFileName(file),
                        ContentType = GetFileType(ctx.FileSystem.Path.GetExtension(file)),
                        RawData = fs,
                    }, ct);
                    logger.LogInformation("Uploaded asset {File}", ctx.FileSystem.Path.GetFileName(file));
                }
            }
            catch
            {
                logger.LogError("Asset upload failed; draft release {Id} left unpublished", release.Id);
                throw;
            }
        }

        var updateRelease = release.ToUpdate();
        updateRelease.Draft = false;
        await client.Repository.Release.Edit(config.Owner, config.RepositoryName, release.Id, updateRelease);
        logger.LogInformation("Published release {Name}", release.Name);
    }

    private static string GetFileType(string ext)
    {
        return ext.ToLowerInvariant() switch
        {
            ".gz" => "application/gzip",
            ".zip" => "application/zip",
            ".txt" => "text/plain",
            _ => "application/octet-stream",
        };
    }
}
