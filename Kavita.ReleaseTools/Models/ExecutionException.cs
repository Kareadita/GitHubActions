using System;

namespace Kavita.ReleaseTools.Models;

public class ExecutionException: Exception
{
    public ExecutionException(string message) : base(message)
    {
    }
}
