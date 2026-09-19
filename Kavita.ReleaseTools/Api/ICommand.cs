using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Api;

/**
 * Re-usable operations that happen on their own, regardless of the stage they happen in
 * Stage specific actions should not be a command
 */
public interface ICommand
{
    Task RunAsync(ExecutionContext ctx, CancellationToken ct);
}
