using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Api;
using LibGit2Sharp;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Commands.Git;

public class GitCommitCommand: RunnableCommand
{
    public override string Name => nameof(GitCommitCommand);

    private readonly string _commitMessage;
    private readonly IReadOnlyList<string> _files;
    private readonly CommitOptions _commitOptions;

    private GitCommitCommand(string commitMessage, IReadOnlyList<string> files, CommitOptions commitOptions): base(false)
    {
        _commitMessage = commitMessage;
        _files = files;
        _commitOptions = commitOptions;
    }

    protected override Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct)
    {
        foreach (var file in _files)
        {
            ctx.Git.Repository.Index.Add(file);
        }

        var signature = ctx.Git.Signature();
        ctx.Git.Repository.Commit(_commitMessage, signature, signature, _commitOptions);

        return Task.CompletedTask;
    }

    public class Builder: ICommandBuilder
    {
        private string _commitMessage = string.Empty;
        private List<string> _files = [];
        private CommitOptions _commitOptions = new();

        public Builder WithCommitMessage(string commitMessage)
        {
            _commitMessage = commitMessage;
            return this;
        }

        public Builder WithFiles(List<string> files)
        {
            _files = files;
            return this;
        }

        public Builder WithFile(string? file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return this;
            }

            _files.Add(file);
            return this;
        }

        public Builder WithCommitOptions(CommitOptions commitOptions)
        {
            _commitOptions = commitOptions;
            return this;
        }

        public ICommand Build()
        {
            if (string.IsNullOrEmpty(_commitMessage))
            {
                throw new ArgumentException("CommitMessage cannot be null or empty");
            }

            return new GitCommitCommand(_commitMessage, _files, _commitOptions);
        }
    }

}
