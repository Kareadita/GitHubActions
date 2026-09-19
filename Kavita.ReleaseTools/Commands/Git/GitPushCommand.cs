using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using LibGit2Sharp;
using Serilog;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Commands.Git;

public class GitPushCommand : MutatingCommand
{
    protected override string Name => nameof(GitPushCommand);
    protected override ILogger Logger => Log.ForContext<GitPushCommand>();

    private readonly string _remote;
    private readonly string _branchName;

    private GitPushCommand(string remote, string branchName) : base(false)
    {
        _remote = remote;
        _branchName = branchName;
    }

    protected override Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        var pushOptions = new PushOptions { CredentialsProvider = ctx.Git.CredentialsHandler };

        var remote = ctx.Git.Repository.Network.Remotes[_remote];

        ctx.Git.Repository.Network.Push(remote, $"refs/heads/{_branchName}", pushOptions);

        return Task.CompletedTask;
    }

    public class Builder: ICommandBuilder
    {
        private string _remote = "origin";
        private string _branchName = "main";

        public Builder WithRemote(string remote)
        {
            _remote = remote;
            return this;
        }

        public Builder WithBranchName(string branchName)
        {
            _branchName = branchName;
            return this;
        }

        public ICommand Build()
        {
            return new GitPushCommand(_remote, _branchName);
        }
    }

}
