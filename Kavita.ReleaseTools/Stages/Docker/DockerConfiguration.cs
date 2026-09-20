using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;

namespace Kavita.ReleaseTools.Stages.Docker;

public class DockerConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Push images upstream
    /// </summary>
    [Required]
    public bool Push { get; init; } = false;

    /// <summary>
    /// Load into local docker registry
    /// </summary>
    [Required]
    public bool Load { get; init; } = false;

    /// <summary>
    /// Docker context
    /// </summary>
    [Required]
    public string Context { get; init; } = ".";

    /// <summary>
    /// Docker file
    /// </summary>
    public string File { get; init; } = string.Empty;

    /// <summary>
    /// For which platforms to build
    /// </summary>
    [Required]
    public List<string> Platforms { get; init; } = [];

    /// <summary>
    /// Image tags. You may use the placeholder {Version} to insert the current version
    /// </summary>
    [Required]
    public Dictionary<ReleaseType, List<string>> Tags { get; init; } = [];

    /// <summary>
    /// Image names (Becomes a Matrix with <see cref="Tags"/>)
    /// </summary>
    [Required]
    public List<string> Images { get; init; } = [];

    /// <summary>
    /// LogLevel of the docker build command
    /// </summary>
    [Required]
    [EnumDataType(typeof(LogLevel))]
    public LogLevel LogLevel { get; init; } = LogLevel.Debug;
}
