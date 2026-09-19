using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Serilog;
using Models_ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Commands;

/// <summary>
/// Use as base class for any commands that make changes that aren't local (Pushes, upstream edits, notifications)
/// </summary>
/// <remarks>Execution is skipped during a dry-run</remarks>
public abstract class MutatingCommand(bool testable): ICommand
{
    protected abstract string Name { get; }
    protected abstract ILogger Logger { get; }

    public Task RunAsync(Models_ExecutionContext ctx, CancellationToken ct)
    {
        if (ctx.IsTest && !testable) return Task.CompletedTask;

        if (ctx.Configuration.DryRun)
        {
            Logger.Information("[DRY RUN] Skipping execution of {Name}...", Name);
            return Task.CompletedTask;
        }

        return ExecuteAsync(ctx, ct);
    }

    protected abstract Task ExecuteAsync(Models_ExecutionContext ctx, CancellationToken ct);

}
