using System;
using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Kavita.ReleaseTools.Models;

public class GitContext: IDisposable
{

    public required Repository Repository { get; init; }

    public required string GitAuthorName { get; init; }
    public required string GitAuthorEmail { get; init; }

    public required CredentialsHandler CredentialsHandler { get; init; }

    public Signature Signature(DateTime? date = null) => new(GitAuthorName, GitAuthorEmail, date ?? DateTime.Now);

    /// <summary>
    /// Whether git considers <paramref name="path"/> to differ from what is committed - an untracked
    /// file counts as a change, a deleted one too
    /// </summary>
    public bool HasChanges(string path)
    {
        var status = Repository.RetrieveStatus(new StatusOptions
        {
            PathSpec = [path],
            IncludeUntracked = true,
        });

        return status.Any(entry => entry.State != FileStatus.Unaltered);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Repository.Dispose();
    }
}
