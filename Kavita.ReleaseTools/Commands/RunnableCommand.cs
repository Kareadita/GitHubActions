using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using Models_ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Commands;

/// <summary>
/// Use for commands that only create local changes
/// </summary>
public abstract class RunnableCommand(bool testable): ICommand
{
    public abstract string Name { get; }

    public Task RunAsync(Models_ExecutionContext ctx, CancellationToken ct)
    {
        if (ctx.IsTest && !testable) return Task.CompletedTask;

        // TODO: Verbose logging with name

        return ExecuteAsync(ctx, ct);
    }

    protected abstract Task ExecuteAsync(Models_ExecutionContext ctx, CancellationToken ct);
}
