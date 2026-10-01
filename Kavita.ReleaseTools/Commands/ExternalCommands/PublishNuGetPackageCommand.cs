using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Stages.PublishToNuGet;
using Serilog;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Commands.ExternalCommands;

public class PublishNuGetPackageCommand(IProcessRunner runner, PublishToNuGetConfiguration config) : MutatingCommand(false)
{
    private const string Dotnet = "dotnet";

    protected override string Name =>  nameof(PublishNuGetPackageCommand);
    protected override ILogger Logger => Log.ForContext<PublishNuGetPackageCommand>();
    protected override Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        return new ProcessCommand.Builder(runner)
            .WithExecutable(Dotnet)
            .WithArguments("nuget", "push", $"{config.OutputDirectory}/*.nupkg", "--api-key", config.ApiKey, "--source", config.Source)
            .AppendArgumentIf(config.SkipDuplicate, "--skip-duplicate")
            .Build()
            .RunAsync(ctx, ct);
    }
}
