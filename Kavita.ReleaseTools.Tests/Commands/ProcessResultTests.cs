using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Tests.Commands;

public class ProcessResultTests
{
    private static ProcessResult Result(int exitCode, string standardOutput = "", string standardError = "") => new()
    {
        Executable = "dotnet",
        Arguments = ["build", "App.csproj"],
        ExitCode = exitCode,
        StandardOutput = standardOutput,
        StandardError = standardError,
    };

    [Fact]
    public void ThrowIfFailed_IsSilent_OnSuccess()
    {
        Result(0).ThrowIfFailed();
    }

    [Fact]
    public void ThrowIfFailed_ReportsTheCommandAndBothStreams()
    {
        var exception = Assert.Throws<ExecutionException>(() =>
            Result(1, standardOutput: "Build FAILED.", standardError: "error CS1002: ; expected").ThrowIfFailed());

        Assert.Contains("dotnet build App.csproj", exception.Message);
        Assert.Contains("exited with code 1", exception.Message);
        Assert.Contains("error CS1002", exception.Message);
        Assert.Contains("Build FAILED.", exception.Message);
    }

    [Fact]
    public void ThrowIfFailed_KeepsTheEndOfAVeryLongOutput()
    {
        var exception = Assert.Throws<ExecutionException>(() =>
            Result(1, standardOutput: new string('a', 5000) + "the actual error").ThrowIfFailed());

        Assert.Contains("the actual error", exception.Message);
        Assert.True(exception.Message.Length < 5000, $"Message was {exception.Message.Length} characters");
    }

    [Fact]
    public void CommandLine_JoinsTheArguments()
    {
        Assert.Equal("dotnet build App.csproj", Result(0).CommandLine);
    }
}
