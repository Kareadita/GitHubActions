using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Models_ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Commands;

/// <summary>
/// Use as base class for any commands that make changes that aren't local (Pushes, upstream edits, notifications)
/// </summary>
/// <remarks>Execution is skipped during a dry-run</remarks>
public abstract class MutatingCommand(bool testable): ICommand
{
    public abstract string Name { get; }

    public Task RunAsync(Models_ExecutionContext ctx, CancellationToken ct)
    {
        if (ctx.IsTest && !testable) return Task.CompletedTask;

        if (ctx.DryRun)
        {
            // TODO: Print dry run info nicely
            return Task.CompletedTask;
        }

        return ExecuteAsync(ctx, ct);
    }

    protected abstract Task ExecuteAsync(Models_ExecutionContext ctx, CancellationToken ct);

}
