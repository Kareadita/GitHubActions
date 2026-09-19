using System;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Commands.Git;

public class GitPushCommand() : MutatingCommand(false)
{
    public override string Name => nameof(GitPushCommand);

    protected override Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
