using System;
using LibGit2Sharp;

namespace Kavita.ReleaseTools.Models;

public class GitContext: IDisposable
{

    public required Repository Repository { get; init; }

    public required string GitAuthorName { get; init; }
    public required string GitAuthorEmail { get; init; }

    public Signature Signature(DateTime? date = null) => new(GitAuthorName, GitAuthorEmail, date ?? DateTime.Now);

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Repository.Dispose();
    }
}
