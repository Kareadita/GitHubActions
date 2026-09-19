using System;
using System.IO.Abstractions;
using Kavita.ReleaseTools.Stages.VersionBump;

namespace Kavita.ReleaseTools.Models;

public class ExecutionContext: IDisposable
{
    public bool IsTest { get; init; } = false;

    /// <summary>
    /// The release being created. Set in <see cref="VersionBumpStage"/>
    /// </summary>
    public Version? ReleaseVersion { get; set; }

    public required IFileSystem FileSystem { get; init; }
    public required ReleaseConfiguration Configuration { get; init; }

    public required GitContext Git { get; init; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Git?.Dispose();
    }
}
