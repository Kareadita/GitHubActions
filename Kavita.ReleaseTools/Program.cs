
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using Kavita.ReleaseTools;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Models;
using Kavita.ReleaseTools.Stages.BuildFrontend;
using Kavita.ReleaseTools.Stages.BuildLibrary;
using Kavita.ReleaseTools.Stages.BuildServer;
using Kavita.ReleaseTools.Stages.Docker;
using Kavita.ReleaseTools.Stages.FlushGitChanges;
using Kavita.ReleaseTools.Stages.GenerateOpenApi;
using Kavita.ReleaseTools.Stages.NotifyDiscord;
using Kavita.ReleaseTools.Stages.PublishToNuGet;
using Kavita.ReleaseTools.Stages.VersionBump;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog(Log.Logger);
});

services.AddSingleton<IProcessRunner, ProcessRunner>();
services.AddScoped<IStage, VersionBumpStage>();
services.AddScoped<IStage, GenerateOpenApiStage>();
services.AddScoped<IStage, FlushGitChangesStage>();
services.AddScoped<IStage, BuildFrontendStage>();
services.AddScoped<IStage, BuildServerStage>();
services.AddScoped<IStage, BuildLibraryStage>();
services.AddScoped<IStage, PublishToNuGetStage>();
services.AddScoped<IStage, DockerStage>();
services.AddScoped<IStage, NotifyDiscordStage>();

var provider = services.BuildServiceProvider();

var stages = provider.GetServices<IStage>().ToList();

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

var repository = new Repository("./");
var gitContext = new GitContext
{
    Repository = repository,
    GitAuthorName = "github-actions[bot]",
    GitAuthorEmail = "github-actions[bot]@users.noreply.github.com",
    CredentialsHandler = (_, _, _) => new UsernamePasswordCredentials
    {
        Username = configuration.GitData.AuthToken,
    }
};

using var executionContext = new ExecutionContext
{
    FileSystem = fs,
    Configuration = configuration,
    Git = gitContext
};

foreach (var stage in stages)
{
    try
    {
        await stage.ExecuteAsync(executionContext, CancellationToken.None);
    }
    catch (Exception exception)
    {
        FailureReport.Render(stage.Name, exception);
        return 1;
    }
}

return 0;
