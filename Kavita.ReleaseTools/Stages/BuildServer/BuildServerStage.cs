using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.BuildServer;

public class BuildServerStage(ILogger<BuildServerStage> logger, IProcessRunner runner, HttpClient httpClient) : ConfiguredStage<BuildServerConfiguration>(logger)
{
    private const string Dotnet = "dotnet";

    // 0644
    private const UnixFileMode DefaultFileMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite |
        UnixFileMode.GroupRead | UnixFileMode.OtherRead;

    // 0755
    private const UnixFileMode DefaultDirMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
        UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
        UnixFileMode.OtherRead | UnixFileMode.OtherExecute;

    private const UnixFileMode ExecBits =
        UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;

    public override string Name => nameof(BuildServerStage);

    protected override BuildServerConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.BuildServer;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, BuildServerConfiguration config)
    {
        List<ValidationIssue> issues = [];

        if (!ctx.FileSystem.File.Exists(config.SlnPath))
            issues.Add(Issue($"SlnPath does not found on disk ({config.SlnPath})"));

        if (!ctx.FileSystem.File.Exists(config.CsprojPath))
            issues.Add(Issue($"CsprojPath does not found on disk ({config.CsprojPath})"));

        if (!config.OutputPath.EndsWith('/'))
            issues.Add(Issue($"OutputPath does not end with /"));

        return issues;
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, BuildServerConfiguration config, CancellationToken ct)
    {
        var buildPath = ctx.FileSystem.Path.Combine(ctx.FileSystem.Path.GetTempPath(), $"{config.AppName}-release-{Guid.NewGuid():N}");

        var rids = config.Rids
            .Where(kv => ctx.ReleaseTypes.Contains(kv.Key))
            .SelectMany(kv => kv.Value)
            .Distinct();

        foreach (var rid in rids)
        {
            await PackageRid(ctx, config, buildPath, rid, ct);
        }
    }

    private async Task PackageRid(ExecutionContext ctx, BuildServerConfiguration config, string buildPath, string rid, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("Starting build process for {Rid}", rid);

        var outputPath = ctx.FileSystem.Path.Combine(buildPath, rid, config.AppName);

        await Build(ctx, config, rid, outputPath, ct);

        DeleteFiles(ctx, config, outputPath);
        CopyFiles(ctx, config, outputPath);
        RenameFiles(ctx, config, outputPath);

        var executables = await IncludeExternalFiles(ctx, config, rid, outputPath, ct);

        executables.Add(config.AppName);
        await PackageBuild(ctx, config, rid, outputPath, executables, ct);

        sw.Stop();
        logger.LogInformation("Completed build process for {Rid} in {TotalSeconds}s", rid, sw.Elapsed.TotalSeconds);
    }

    private async Task Build(ExecutionContext ctx, BuildServerConfiguration config, string rid, string outputPath, CancellationToken ct)
    {
        await new ProcessCommand.Builder(runner)
            .WithLogLevel(config.LogLevel)
            .WithExecutable(Dotnet)
            .WithArguments("msbuild", "-restore", config.SlnPath, $"-p:Configuration={config.Configuration}", "-p:Platform=\"Any CPU\"", $"-p:RuntimeIdentifiers={rid}")
            .Build()
            .RunAsync(ctx, ct);

        await new ProcessCommand.Builder(runner)
            .WithLogLevel(config.LogLevel)
            .WithExecutable(Dotnet)
            .WithArguments("publish", config.CsprojPath, "-c", config.Configuration, "--no-restore", "--runtime", rid, "-o", outputPath)
            .AppendArgumentIf(config.SelfContained, "--self-contained")
            .Build()
            .RunAsync(ctx, ct);
    }

    private async Task PackageBuild(ExecutionContext ctx, BuildServerConfiguration config, string rid, string outputPath, IReadOnlySet<string> executables, CancellationToken ct)
    {
        var fs = ctx.FileSystem;
        logger.LogTrace("Compressing {Path}", outputPath);

        if (!fs.Directory.Exists(config.OutputPath))
            fs.Directory.CreateDirectory(config.OutputPath);

        var archivePath = $"{config.OutputPath}{config.AppName.ToLowerInvariant()}-{rid}.tar.gz";

        await using var fileStream = fs.File.Create(archivePath);
        await using var gzip = new GZipStream(fileStream, CompressionLevel.Optimal);
        await using var tar = new TarWriter(gzip, TarEntryFormat.Pax);

        await tar.WriteEntryAsync(new PaxTarEntry(TarEntryType.Directory, $"{config.AppName}/")
        {
            Mode = DefaultDirMode,
        }, ct);

        foreach (var path in fs.Directory.EnumerateFileSystemEntries(outputPath, "*", SearchOption.AllDirectories))
        {
            var relative = NormalizeRelative(fs.Path.GetRelativePath(outputPath, path));
            var entryName = $"{config.AppName}/{relative}";

            if (fs.Directory.Exists(path))
            {
                await tar.WriteEntryAsync(new PaxTarEntry(TarEntryType.Directory, entryName + "/")
                {
                    Mode = DefaultDirMode,
                }, ct);
                continue;
            }

            // Keep file mode when possible
            var mode = OperatingSystem.IsWindows() ? DefaultFileMode : fs.File.GetUnixFileMode(path);

            if (executables.Contains(relative))
            {
                mode |= ExecBits;
            }

            await using var data = fs.File.OpenRead(path);
            await tar.WriteEntryAsync(new PaxTarEntry(TarEntryType.RegularFile, entryName)
            {
                Mode = mode,
                DataStream = data,
            }, ct);
        }
    }

    private void DeleteFiles(ExecutionContext ctx, BuildServerConfiguration config, string outputPath)
    {
        foreach (var path in config.DeletePaths)
        {
            var completePath = ctx.FileSystem.Path.Combine(outputPath, path);

            logger.LogTrace("Deleting {Path}", completePath);
            if (ctx.FileSystem.Directory.Exists(completePath))
            {
                ctx.FileSystem.Directory.Delete(completePath, true);
            }
            else if (ctx.FileSystem.File.Exists(completePath))
            {
                ctx.FileSystem.File.Delete(completePath);
            }
        }
    }

    private void CopyFiles(ExecutionContext ctx, BuildServerConfiguration config, string outputPath)
    {
        foreach (var fromTo in config.CopyFiles)
        {
            var destPath = ctx.FileSystem.Path.Combine(outputPath, fromTo.To);
            if (ctx.FileSystem.File.Exists(fromTo.From))
            {
                var directory = ctx.FileSystem.Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(directory) && !ctx.FileSystem.Directory.Exists(directory))
                    ctx.FileSystem.Directory.CreateDirectory(directory);

                logger.LogTrace("Copying {From} to {To}", fromTo.To, destPath);
                if (ctx.FileSystem.File.Exists(destPath))
                    ctx.FileSystem.File.Delete(destPath);

                ctx.FileSystem.File.Copy(fromTo.From, destPath);
            }
            else
            {
                logger.LogWarning("Cannot find configured file at path {Path} to copy, skipping", fromTo.From);
            }
        }
    }

    private void RenameFiles(ExecutionContext ctx, BuildServerConfiguration config, string outputPath)
    {
        foreach (var fromTo in config.RenameFiles)
        {
            var srcPath = ctx.FileSystem.Path.Combine(outputPath, fromTo.From);
            var destPath = ctx.FileSystem.Path.Combine(outputPath, fromTo.To);
            if (ctx.FileSystem.File.Exists(srcPath))
            {
                logger.LogTrace("Renaming {From} to {To}", srcPath, destPath);
                ctx.FileSystem.File.Move(srcPath, destPath);
            }
            else
            {
                logger.LogWarning("Cannot find configured file at path {Path} to rename", srcPath);
            }
        }
    }

    private async Task<HashSet<string>> IncludeExternalFiles(ExecutionContext ctx, BuildServerConfiguration config, string rid, string outputPath, CancellationToken ct)
    {
        var executables = new HashSet<string>(StringComparer.Ordinal);

        foreach (var externalFile in config.IncludeExternalFiles.GetValueOrDefault(rid, []))
        {
            var destPath = ctx.FileSystem.Path.Combine(outputPath, externalFile.To);

            logger.LogDebug("Including external file {DestPath} from {Url}", destPath, externalFile.From);

            var response = await httpClient.GetAsync(externalFile.From, ct);
            response.EnsureSuccessStatusCode();

            var dest = ctx.FileSystem.File.OpenWrite(destPath);
            await response.Content.CopyToAsync(dest, ct);

            dest.Close();

            if (!string.IsNullOrEmpty(externalFile.Sha256))
            {
                var downloadedSha256 = await ComputeSha256Async(ctx, destPath, ct);

                if (!downloadedSha256.Equals(externalFile.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogError("The downloaded file's sha is not what was expected! Wanted: {Wanted}, got: {Actual}",
                        externalFile.Sha256, downloadedSha256);
                    throw new ExecutionException("External file has an incorrect SHA, stopping build");
                }

                logger.LogInformation("{Path} passed SHA256 check {Sha}", destPath, downloadedSha256);
            }

            if (externalFile.Executable)
            {
                executables.Add(NormalizeRelative(externalFile.To));
            }
        }

        return executables;
    }

    private async Task<string> ComputeSha256Async(ExecutionContext ctx, string path, CancellationToken cancellationToken)
    {
        await using var stream = ctx.FileSystem.File.OpenRead(path);

        var hash = await SHA256.HashDataAsync(stream, cancellationToken);

        return Convert.ToHexString(hash);
    }

    private static string NormalizeRelative(string path) => path.Replace('\\', '/').TrimStart('/');
}
