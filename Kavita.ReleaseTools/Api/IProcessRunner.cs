using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Api;

public interface IProcessRunner
{
    /// <param name="arguments"></param>
    /// <param name="workingDirectory">Directory to run in, or null to inherit the current one</param>
    /// <param name="executable"></param>
    /// <param name="ct"></param>
    Task<ProcessResult> RunAsync(string executable, IReadOnlyList<string> arguments, string? workingDirectory, CancellationToken ct);
}
