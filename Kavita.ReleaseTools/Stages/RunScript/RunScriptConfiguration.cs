using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;

namespace Kavita.ReleaseTools.Stages.RunScript;

public class RunScriptConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Executable to use
    /// </summary>
    [Required]
    public required string Executable { get; init; }

    /// <summary>
    /// Arguments to pass down
    /// </summary>
    [Required]
    public required List<string> Arguments { get; init; }

    /// <summary>
    /// Set the working directory. Defaults to PWD
    /// </summary>
    public required string? WorkingDirectory { get; init; } = null;

    /// <summary>
    /// Override at which log level the output is logged
    /// </summary>
    [EnumDataType(typeof(LogLevel))]
    public LogLevel LogLevel { get; init; } = LogLevel.Trace;
}
