using System;
using System.IO.Abstractions;
using Kavita.ReleaseTools.Stages.VersionBump;

namespace Kavita.ReleaseTools.Models;

public class ExecutionContext: IDisposable
{
    public bool DryRun { get; init; }
    public bool IsTest { get; init; }

    /// <summary>
    /// The release being created. Set in <see cref="VersionBumpStage"/>
    /// </summary>
    public Version? ReleaseVersion { get; set; }

    public IFileSystem FileSystem { get; init; }
    public ReleaseConfiguration Configuration { get; init; }

    public GitContext Git { get; init; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Git.Dispose();
    }
}
