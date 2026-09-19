using System;
using System.IO.Abstractions;

namespace Kavita.ReleaseTools.Models;

public class ExecutionContext: IDisposable
{
    public bool DryRun { get; init; }
    public bool IsTest { get; init; }

    public IFileSystem FileSystem { get; init; }
    public ReleaseConfiguration Configuration { get; init; }

    public GitContext Git { get; init; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Git.Dispose();
    }
}
