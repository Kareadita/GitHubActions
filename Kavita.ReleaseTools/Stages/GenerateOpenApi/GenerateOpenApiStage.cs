using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Commands.Git;
using Kavita.ReleaseTools.Models;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.GenerateOpenApi;

/// <summary>
/// Generates the OpenAPI (swagger) spec for a project, and optionally commits it when it changed
/// </summary>
public partial class GenerateOpenApiStage(ILogger<GenerateOpenApiStage> logger, IProcessRunner processRunner)
    : ConfiguredStage<GenerateOpenApiConfiguration>(logger)
{
    private const string Dotnet = "dotnet";
    private const string SwashbucklePackageId = "Swashbuckle.AspNetCore";
    private const string DefaultCommitMessage = "Update OpenAPI documentation";

    public override string Name => nameof(GenerateOpenApiStage);

    protected override GenerateOpenApiConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.GenerateOpenApi;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, GenerateOpenApiConfiguration config)
    {
        List<ValidationIssue> issues =
        [
            .. ValidationHelpers.ValidateCsprojPath(ctx, Name, config.CsprojPath),
            .. ValidationHelpers.ValidateCsprojPath(ctx, Name, config.SwashbuckleVersionSource, "swashbuckleVersionSource"),
        ];

        if (string.IsNullOrWhiteSpace(config.OutputPath))
        {
            issues.Add(Issue("outputPath is not configured"));
        }

        if (string.IsNullOrWhiteSpace(config.DocumentName))
        {
            issues.Add(Issue("documentName is not configured"));
        }

        if (string.IsNullOrWhiteSpace(config.Configuration))
        {
            issues.Add(Issue("configuration is not configured"));
        }

        issues.AddRange(ValidateSwashbuckleVersion(ctx, config));

        return issues;
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, GenerateOpenApiConfiguration config, CancellationToken ct)
    {
        var existingSpec = await ReadSpecAsync(ctx, config.OutputPath, ct);

        await new ProcessCommand.Builder(processRunner)
            .WithExecutable(Dotnet)
            .WithArguments("build", config.CsprojPath, "--configuration", config.Configuration)
            .Build()
            .RunAsync(ctx, ct);

        var assemblyPath = ResolveAssemblyPath(ctx, config);
        var swashbuckleVersion = ResolveSwashbuckleVersion(ctx, config);

        var generateCommand = new GenerateOpenApiDocCommand(processRunner, swashbuckleVersion, assemblyPath,
            config.OutputPath, config.DocumentName);
        await generateCommand.RunAsync(ctx, ct);

        var generatedSpec = await ReadSpecAsync(ctx, config.OutputPath, ct)
            ?? throw new ExecutionException($"The swagger CLI reported success, but no spec was written to {config.OutputPath}");

        if (IsUnchanged(existingSpec, generatedSpec))
        {
            logger.LogInformation("{OutputPath} is unchanged, nothing to commit", config.OutputPath);
            return;
        }

        logger.LogInformation("Generated {OutputPath}", config.OutputPath);

        if (!config.Commit) return;

        var commitCommand = new GitCommitCommand.Builder()
            .WithCommitMessage(string.IsNullOrWhiteSpace(config.CommitMessage) ? DefaultCommitMessage : config.CommitMessage)
            .WithCommitOptions(new CommitOptions { AllowEmptyCommit = false })
            .WithFile(config.OutputPath)
            .Build();
        await commitCommand.RunAsync(ctx, ct);
    }

    /// <summary>
    /// Reads the version of <paramref name="packageId"/> out of a csproj
    /// </summary>
    /// <returns>
    /// Null when the package is not referenced, or is referenced without a version - central package
    /// management keeps the version elsewhere, which this does not follow
    /// </returns>
    private static string? FindPackageVersion(string csprojContent, string packageId)
    {
        var elements = PackageReferencePattern().Matches(csprojContent);

        for (var i = 0; i < elements.Count; i++)
        {
            var element = elements[i].Value;
            var reference = ReadAttribute(element, "Include") ?? ReadAttribute(element, "Update");

            if (!string.Equals(reference, packageId, StringComparison.OrdinalIgnoreCase)) continue;

            var version = ReadAttribute(element, "Version");
            if (!string.IsNullOrWhiteSpace(version)) return version;
        }

        return null;
    }

    /// <summary>
    /// The assembly a project produces: its AssemblyName, or the project file name when it doesn't
    /// set one
    /// </summary>
    private static string ResolveAssemblyName(string csprojContent, string csprojPath)
    {
        var match = AssemblyNamePattern().Match(csprojContent);
        var configured = match.Success ? match.Groups["name"].Value.Trim() : string.Empty;

        return configured.Length > 0 ? configured : Path.GetFileNameWithoutExtension(csprojPath);
    }

    /// <summary>
    /// Locates the built assembly next to the project: bin/&lt;configuration&gt;/&lt;name&gt;.dll, or one
    /// directory deeper when the target framework is part of the output path
    /// </summary>
    /// <returns>Every match - more than one means the build output needs cleaning up, not guessing</returns>
    private static IReadOnlyList<string> FindBuiltAssemblies(IFileSystem fs, string csprojPath, string configuration,
        string assemblyName)
    {
        var binDirectory = fs.Path.Combine(fs.Path.GetDirectoryName(csprojPath) ?? ".", "bin", configuration);
        if (!fs.Directory.Exists(binDirectory)) return [];

        List<string> assemblies = [];

        var direct = fs.Path.Combine(binDirectory, $"{assemblyName}.dll");
        if (fs.File.Exists(direct)) assemblies.Add(direct);

        foreach (var directory in fs.Directory.EnumerateDirectories(binDirectory))
        {
            var nested = fs.Path.Combine(directory, $"{assemblyName}.dll");
            if (fs.File.Exists(nested)) assemblies.Add(nested);
        }

        return assemblies;
    }

    /// <summary>
    /// Compares two specs ignoring line endings - a checkout with different eol conversion would
    /// otherwise report a change on every single run
    /// </summary>
    private static bool IsUnchanged(string? existing, string generated)
    {
        return existing is not null && NormalizeLineEndings(existing) == NormalizeLineEndings(generated);
    }

    private static string NormalizeLineEndings(string content) => content.Replace("\r\n", "\n");

    private static string? ReadAttribute(string element, string attributeName)
    {
        foreach (Match attribute in AttributePattern().Matches(element))
        {
            if (attribute.Groups["name"].Value.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
            {
                return attribute.Groups["value"].Value;
            }
        }

        return null;
    }

    private List<ValidationIssue> ValidateSwashbuckleVersion(ValidationContext ctx, GenerateOpenApiConfiguration config)
    {
        // Reported as a missing path already, nothing to read
        if (string.IsNullOrWhiteSpace(config.SwashbuckleVersionSource) ||
            !ctx.FileSystem.File.Exists(config.SwashbuckleVersionSource))
        {
            return [];
        }

        var content = ctx.FileSystem.File.ReadAllText(config.SwashbuckleVersionSource);
        if (FindPackageVersion(content, SwashbucklePackageId) is not null) return [];

        return
        [
            Issue($"{SwashbucklePackageId} is not referenced with a Version in {config.SwashbuckleVersionSource}, " +
                  "so the version the swagger CLI must match cannot be determined"),
        ];
    }

    private static string ResolveAssemblyPath(ExecutionContext ctx, GenerateOpenApiConfiguration config)
    {
        var csprojContent = ctx.FileSystem.File.ReadAllText(config.CsprojPath);
        var assemblyName = ResolveAssemblyName(csprojContent, config.CsprojPath);
        var assemblies = FindBuiltAssemblies(ctx.FileSystem, config.CsprojPath, config.Configuration, assemblyName);

        return assemblies.Count switch
        {
            1 => assemblies[0],
            0 => throw new ExecutionException(
                $"No build output found for {config.CsprojPath}, expected {assemblyName}.dll under bin/{config.Configuration}"),
            _ => throw new ExecutionException(
                $"Found {assemblies.Count} builds of {assemblyName}.dll under bin/{config.Configuration}: " +
                $"{string.Join(", ", assemblies)}. Clean up the stale build output, or build a single target framework"),
        };
    }

    private static string ResolveSwashbuckleVersion(ExecutionContext ctx, GenerateOpenApiConfiguration config)
    {
        var content = ctx.FileSystem.File.ReadAllText(config.SwashbuckleVersionSource);

        return FindPackageVersion(content, SwashbucklePackageId)
               ?? throw new ExecutionException(
                   $"No {SwashbucklePackageId} PackageReference with a Version found in {config.SwashbuckleVersionSource}");
    }

    private static async Task<string?> ReadSpecAsync(ExecutionContext ctx, string outputPath, CancellationToken ct)
    {
        return ctx.FileSystem.File.Exists(outputPath)
            ? await ctx.FileSystem.File.ReadAllTextAsync(outputPath, ct)
            : null;
    }

    [GeneratedRegex(@"<PackageReference\b[^>]*>", RegexOptions.Compiled)]
    private static partial Regex PackageReferencePattern();

    [GeneratedRegex(@"(?<name>[\w]+)\s*=\s*""(?<value>[^""]*)""", RegexOptions.Compiled)]
    private static partial Regex AttributePattern();

    [GeneratedRegex(@"<AssemblyName>\s*(?<name>[^<]*?)\s*</AssemblyName>", RegexOptions.Compiled)]
    private static partial Regex AssemblyNamePattern();
}
