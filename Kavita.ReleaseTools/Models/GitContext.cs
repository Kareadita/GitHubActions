using System;
using System.Linq;
using LibGit2Sharp;

namespace Kavita.ReleaseTools.Models;

public class GitContext: IDisposable
{

    public required Repository Repository { get; init; }

    public required string GitAuthorName { get; init; }
    public required string GitAuthorEmail { get; init; }

    public Signature Signature(DateTime? date = null) => new(GitAuthorName, GitAuthorEmail, date ?? DateTime.Now);

    /// <summary>
    /// Whether git considers <paramref name="path"/> to differ from what is committed - an untracked
    /// file counts as a change, a deleted one too
    /// </summary>
    /// <remarks>
    /// Asks the status rather than comparing bytes, so the answer accounts for the eol and filter
    /// rules git applies when staging. Comparing the file's contents directly would report a change
    /// that git then refuses to commit
    /// </remarks>
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
