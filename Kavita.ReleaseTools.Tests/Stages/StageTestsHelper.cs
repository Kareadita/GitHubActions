using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Kavita.ReleaseTools.Models;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Tests.Stages;

public static class StageTestsHelper
{

    public static ExecutionContext CreateExecutionContext(ReleaseConfiguration releaseConfiguration, IFileSystem? fileSystem = null)
    {
        return new ExecutionContext
        {
            DryRun = false,
            IsTest = true,
            FileSystem = fileSystem ?? new MockFileSystem(),
            Configuration = releaseConfiguration,
            Git = null!
        };
    }

}
