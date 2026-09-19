using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Stages.GenerateOpenApi;

/// <summary>
/// A stage that allows for the generation of the opanapi spec (swagger)
/// </summary>
public class GenerateOpenApiConfiguration: IStageConfiguration
{
    public bool Disabled { get; init; }

    /// <summary>
    /// Path the to .csproj file that contains the version to bump
    /// </summary>
    public required string CsprojPath { get; init; }
}
