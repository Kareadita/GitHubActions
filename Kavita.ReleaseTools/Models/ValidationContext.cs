using System.IO.Abstractions;

namespace Kavita.ReleaseTools.Models;

public class ValidationContext
{

    public IFileSystem FileSystem { get; }

    public ReleaseConfiguration Configuration { get; set; }
}
