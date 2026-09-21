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
    private readonly string _authToken;

    private GitPushCommand(string remote, string branchName, string authToken) : base(false)
    {
        _remote = remote;
        _branchName = branchName;
        _authToken = authToken;
    }

    protected override Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        var pushOptions = new PushOptions { CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials
            { Username = "x-auth-token", Password = _authToken }
        };

        var remote = ctx.Git.Repository.Network.Remotes[_remote];

        ctx.Git.Repository.Network.Push(remote, $"refs/heads/{_branchName}", pushOptions);

        return Task.CompletedTask;
    }

    public class Builder: ICommandBuilder
    {
        private string _remote = "origin";
        private string _branchName = "main";
        private string _authToken = string.Empty;

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

        public Builder WithAuthToken(string authToken)
        {
            _authToken = authToken;
            return this;
        }

        public ICommand Build()
        {
            return new GitPushCommand(_remote, _branchName, _authToken);
        }
    }

}
