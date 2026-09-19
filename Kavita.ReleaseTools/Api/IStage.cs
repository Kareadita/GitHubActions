using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Models;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Api;

public interface IStage
{
    string Name { get; }

    IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx);
    Task ExecuteAsync(ExecutionContext ctx, CancellationToken ct);
}
