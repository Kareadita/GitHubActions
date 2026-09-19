
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using Kavita.ReleaseTools;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;
using Kavita.ReleaseTools.Stages.FlushGitChanges;
using Kavita.ReleaseTools.Stages.GenerateOpenApi;
using Kavita.ReleaseTools.Stages.VersionBump;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

var stages = new List<IStage>
{
    new VersionBumpStage(),
    new GenerateOpenApiStage(),
    new FlushGitChangesStage(),
};

var fs = new FileSystem();
var configuration = ConfigurationReader.Read(fs, args);

var validationContext = new ValidationContext
{
    FileSystem = fs,
    Configuration = configuration
};

List<ValidationIssue> issues = [];

foreach (var stage in stages)
{
    issues.AddRange(stage.Validate(validationContext));
}

if (issues.Count > 0)
{
    ValidationReport.Render(issues);
    return 1;
}

using var executionContext = new ExecutionContext
{
    FileSystem = fs,
    Configuration = configuration,
    Git = null!
};

foreach (var stage in stages)
{
    await stage.ExecuteAsync(executionContext, CancellationToken.None);
}

return 0;
