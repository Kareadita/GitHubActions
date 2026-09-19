using System;
using LibGit2Sharp;

namespace Kavita.ReleaseTools.Models;

public class GitContext: IDisposable
{

    public Repository Repository { get; private set; }

    public string GitAuthorName { get; private set; }
    public string GitAuthorEmail { get; private set; }

    public Signature Signature(DateTime? date = null) => new(GitAuthorName, GitAuthorEmail, date ?? DateTime.Now);

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Repository.Dispose();
    }
}
