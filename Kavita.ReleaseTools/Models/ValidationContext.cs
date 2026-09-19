using System.IO.Abstractions;

namespace Kavita.ReleaseTools.Models;

public class ValidationContext
{

    public required IFileSystem FileSystem { get; init; }

    public required ReleaseConfiguration Configuration { get; init; }
}
