using System;
using System.Collections.Generic;
using System.CommandLine;
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

    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Kavita release tools");

        rootCommand.Subcommands.Add(RunCommand.Build());
        rootCommand.Subcommands.Add(GenerateSchemaCommand.Build());

        return await rootCommand.Parse(args).InvokeAsync();
    }

}
