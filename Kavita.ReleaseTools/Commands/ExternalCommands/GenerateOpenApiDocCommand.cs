using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Commands.ExternalCommands;

/// <summary>
/// Generates the OpenAPI (swagger) document for an already built assembly, by installing the
/// Swashbuckle CLI and running it against that assembly
/// </summary>
/// <remarks>
/// The CLI version is pinned to the Swashbuckle.AspNetCore version the application references: the
/// CLI loads the application and calls its own swagger generator, so a mismatch fails at runtime.
/// <br/>
/// The tool manifest is created in a throwaway temp directory rather than the repository, so
/// releasing never leaves a .config/dotnet-tools.json behind (or overwrites an existing one). The
/// CLI resolves the application's dependencies from the assembly's own directory, so running from
/// elsewhere is not a problem.
/// </remarks>
public class GenerateOpenApiDocCommand(IProcessRunner runner, string swashbuckleVersion, string assemblyPath, string outputPath, string documentName): RunnableCommand(true)
{
    private const string ToolPackageId = "Swashbuckle.AspNetCore.Cli";
    private const string Dotnet = "dotnet";

    public override string Name => nameof(GenerateOpenApiDocCommand);

    protected override async Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        var toolDirectory = ctx.FileSystem.Path.Combine(ctx.FileSystem.Path.GetTempPath(), "release-tools", $"openapi-{Guid.NewGuid():N}");
        ctx.FileSystem.Directory.CreateDirectory(toolDirectory);

        await RunAsync(toolDirectory, ["new", "tool-manifest"], ct);
        await RunAsync(toolDirectory, ["tool", "install", ToolPackageId, "--version", swashbuckleVersion], ct);

        await RunAsync(toolDirectory,
        [
            "swagger", "tofile",
            "--output", ctx.FileSystem.Path.GetFullPath(outputPath),
            ctx.FileSystem.Path.GetFullPath(assemblyPath),
            documentName,
        ], ct);
    }

    private async Task RunAsync(string workingDirectory, IReadOnlyList<string> arguments, CancellationToken ct)
    {
        var result = await runner.RunAsync(Dotnet, arguments, workingDirectory, LogLevel.Trace, ct);
        result.ThrowIfFailed();
    }
}
