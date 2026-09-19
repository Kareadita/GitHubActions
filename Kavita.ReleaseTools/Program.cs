using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Commands.ExternalCommands;
using Kavita.ReleaseTools.Configuration;
using Kavita.ReleaseTools.Models;
using Kavita.ReleaseTools.Rendering;
using Kavita.ReleaseTools.Stages.BuildFrontend;
using Kavita.ReleaseTools.Stages.BuildLibrary;
using Kavita.ReleaseTools.Stages.BuildServer;
using Kavita.ReleaseTools.Stages.Docker;
using Kavita.ReleaseTools.Stages.FlushGitChanges;
using Kavita.ReleaseTools.Stages.GenerateOpenApi;
using Kavita.ReleaseTools.Stages.NotifyDiscord;
using Kavita.ReleaseTools.Stages.ParseReleaseTypes;
using Kavita.ReleaseTools.Stages.PublishToNuGet;
using Kavita.ReleaseTools.Stages.VersionBump;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools;

public static class Program
{
    private const string Usage = "Usage: releasetools <config.yaml>";

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            await Console.Error.WriteLineAsync(Usage);
            return 1;
        }

        Log.Logger = CreateLogger();

        await using var provider = BuildServiceProvider();
        var fileSystem = new FileSystem();

        if (ReadConfiguration(fileSystem, args[0]) is not { } configuration) return 1;

        var stages = provider.GetServices<IStage>().ToList();

        var issues = Validate(stages, fileSystem, configuration);
        if (issues.Count > 0)
        {
            ValidationReport.Render(issues);
            return 1;
        }

        using var executionContext = new ExecutionContext
        {
            FileSystem = fileSystem,
            Configuration = configuration,
            Git = CreateGitContext(configuration)
        };

        return await ExecuteStagesAsync(stages, executionContext, CancellationToken.None);
    }

    /// <summary>
    /// Reads the config, reporting the reason when it cannot be read
    /// </summary>
    private static ReleaseConfiguration? ReadConfiguration(IFileSystem fileSystem, string configPath)
    {
        try
        {
            return ConfigurationReader.Read(fileSystem, configPath);
        }
        catch (Exception exception)
        {
            FailureReport.Render(nameof(ConfigurationReader), exception);
            return null;
        }
    }

    /// <summary>
    /// Collects everything the configuration and the stages can find wrong before anything runs, so
    /// a broken config reports all of it at once
    /// </summary>
    private static List<ValidationIssue> Validate(IReadOnlyList<IStage> stages, IFileSystem fileSystem,
        ReleaseConfiguration configuration)
    {
        var context = new ValidationContext
        {
            FileSystem = fileSystem,
            Configuration = configuration
        };

        return
        [
            .. ConfigurationReader.Validate(configuration),
            .. stages.SelectMany(stage => stage.Validate(context))
        ];
    }

    private static async Task<int> ExecuteStagesAsync(IReadOnlyList<IStage> stages, ExecutionContext context,
        CancellationToken ct)
    {
        foreach (var stage in stages)
        {
            try
            {
                await stage.ExecuteAsync(context, ct);
            }
            catch (Exception exception)
            {
                FailureReport.Render(stage.Name, exception);
                return 1;
            }
        }

        return 0;
    }

    private static GitContext CreateGitContext(ReleaseConfiguration configuration) => new()
    {
        Repository = new Repository("./"),
        GitAuthorName = "github-actions[bot]",
        GitAuthorEmail = "github-actions[bot]@users.noreply.github.com",
        CredentialsHandler = (_, _, _) => new UsernamePasswordCredentials
        {
            Username = configuration.GitData.AuthToken,
        }
    };

    private static Serilog.ILogger CreateLogger() => new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext} {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger);
        });

        services.AddSingleton<IProcessRunner, ProcessRunner>();

        // Registration order is execution order
        services.AddScoped<IStage, ParseReleaseTypesStage>();
        services.AddScoped<IStage, VersionBumpStage>();
        services.AddScoped<IStage, GenerateOpenApiStage>();
        services.AddScoped<IStage, FlushGitChangesStage>();
        services.AddScoped<IStage, BuildFrontendStage>();
        services.AddScoped<IStage, BuildServerStage>();
        services.AddScoped<IStage, BuildLibraryStage>();
        services.AddScoped<IStage, PublishToNuGetStage>();
        services.AddScoped<IStage, DockerStage>();
        services.AddScoped<IStage, NotifyDiscordStage>();

        return services.BuildServiceProvider();
    }
}
