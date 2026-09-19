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

    [Required]
    public bool Push { get; init; } = false;

    [Required]
    public bool Load { get; init; } = false;

    [Required]
    public string Context { get; init; } = ".";

    public string File { get; init; } = string.Empty;

    [Required]
    public List<string> Platforms { get; init; } = [];

    /// <summary>
    /// You may use the placeholder {Version} to insert the current version
    /// </summary>
    [Required]
    public Dictionary<ReleaseType, List<string>> Tags { get; init; } = [];

    [Required]
    public List<string> Images { get; init; } = [];

    [Required]
    [EnumDataType(typeof(LogLevel))]
    public LogLevel LogLevel { get; init; } = LogLevel.Debug;
}
