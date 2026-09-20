using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Kavita.ReleaseTools.Stages.VersionBump;

namespace Kavita.ReleaseTools.Models;

public class ExecutionContext: IDisposable
{
    public bool IsTest { get; init; } = false;

    /// <summary>
    /// The release being created. Set in <see cref="VersionBumpStage"/>
    /// </summary>
    public Version ReleaseVersion { get; set; } = null!;

    public VersionParseArtifacts VersionParseArtifacts { get; set; } = null!;

    public IReadOnlyList<ReleaseType> ReleaseTypes { get; set; } = [];

    public required IFileSystem FileSystem { get; init; }
    public required ReleaseConfiguration Configuration { get; init; }

    public required GitContext Git { get; init; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Git?.Dispose();
    }
}

public class VersionParseArtifacts
{
    public required string AssemblyContent { get; init; }
    public required Match AssemblyMatch { get; init; }
}
