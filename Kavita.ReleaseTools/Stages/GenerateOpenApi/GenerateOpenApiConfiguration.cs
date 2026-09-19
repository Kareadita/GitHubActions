using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Stages.GenerateOpenApi;

/// <summary>
/// A stage that allows for the generation of the opanapi spec (swagger)
/// </summary>
public class GenerateOpenApiConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <summary>
    /// Path to the project that produces the assembly the spec is generated from
    /// </summary>
    /// <remarks>It is built, so the spec always reflects what that project currently compiles to</remarks>
    public required string CsprojPath { get; init; }

    /// <summary>
    /// Path to a .csproj referencing Swashbuckle.AspNetCore, which the swagger CLI version is read from
    /// </summary>
    /// <remarks>
    /// The CLI is part of Swashbuckle and generates the document with the application's own copy, so
    /// the two have to match
    /// </remarks>
    public required string SwashbuckleVersionSource { get; init; }

    /// <summary>
    /// Where the generated spec is written
    /// </summary>
    public string OutputPath { get; init; } = "openapi.json";

    /// <summary>
    /// Swagger document name to generate
    /// </summary>
    public string DocumentName { get; init; } = "v1";

    /// <summary>
    /// Build configuration used for <see cref="CsprojPath"/>
    /// </summary>
    public string Configuration { get; init; } = "Debug";

    /// <summary>
    /// Commit the generated spec, when it differs from the one already in the repository
    /// </summary>
    public bool Commit { get; init; } = false;

    /// <summary>
    /// Custom commit message. Only relevant if <see cref="Commit"/> is true
    /// </summary>
    public string? CommitMessage { get; init; }
}
