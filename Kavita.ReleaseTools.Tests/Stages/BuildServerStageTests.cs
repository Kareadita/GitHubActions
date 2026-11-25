using System.Formats.Tar;
using System.IO.Abstractions;
using System.Net;
using Kavita.ReleaseTools.Stages.BuildServer;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kavita.ReleaseTools.Tests.Stages;

public class BuildServerStageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "buildserver-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, true);
    }

    private string CreateOutputPath()
    {
        var outputPath = Path.Combine(_root, "linux-x64", "Kavita");
        Directory.CreateDirectory(Path.Combine(outputPath, "sub"));

        File.WriteAllText(Path.Combine(outputPath, "Kavita"), "binary");
        File.WriteAllText(Path.Combine(outputPath, "appsettings.json"), "{}");
        File.WriteAllText(Path.Combine(outputPath, "sub", "tool"), "binary");

        SetMode(Path.Combine(outputPath, "Kavita"), 0b111_101_101);
        SetMode(Path.Combine(outputPath, "appsettings.json"), 0b110_100_100);
        SetMode(Path.Combine(outputPath, "sub", "tool"), 0b110_100_100);

        return outputPath;
    }

    private static void SetMode(string path, int mode)
    {
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(path, (UnixFileMode)mode);
    }

    private Models.ExecutionContext CreateContext()
    {
        return new Models.ExecutionContext
        {
            IsTest = true,
            FileSystem = new FileSystem(),
            Configuration = null!,
            Git = null!,
        };
    }

    private BuildServerConfiguration CreateConfig(string archiveDir, Dictionary<string, List<ExternalFile>>? externalFiles = null)
    {
        return new BuildServerConfiguration
        {
            AppName = "Kavita",
            SlnPath = "x",
            CsprojPath = "x",
            Rids = [],
            IncludeExternalFiles = externalFiles ?? [],
            OutputPath = archiveDir,
        };
    }

    private static async Task<Dictionary<string, TarEntry>> ReadEntriesAsync(string archivePath)
    {
        await using var fs = File.OpenRead(archivePath);
        await using var gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress);
        using var reader = new TarReader(gz);

        var entries = new Dictionary<string, TarEntry>();
        while (await reader.GetNextEntryAsync() is { } entry)
            entries[entry.Name] = entry;
        return entries;
    }

    [Fact]
    public async Task PackageBuild_SetsExecBitsOnExecutablesOnly()
    {
        var outputPath = CreateOutputPath();
        var archiveDir = Path.Combine(_root, "out") + "/";
        Directory.CreateDirectory(archiveDir);

        var stage = new BuildServerStage(NullLogger<BuildServerStage>.Instance, null!, new HttpClient());
        var config = CreateConfig(archiveDir);

        // Deliberately different casing to prove matching is case-insensitive
        var executables = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "KAVITA", "SUB/TOOL" };

        await stage.PackageBuild(CreateContext(), config, "linux-x64", outputPath, executables, CancellationToken.None);

        var entries = await ReadEntriesAsync(Path.Combine(archiveDir, "kavita-linux-x64.tar.gz"));

        Assert.True(entries.ContainsKey("Kavita/"), "root directory entry missing");
        Assert.Equal(0b111_101_101, (int)entries["Kavita/"].Mode);

        Assert.True(entries.ContainsKey("Kavita/Kavita"));
        Assert.True(entries["Kavita/Kavita"].Mode.HasFlag(UnixFileMode.UserExecute), "app executable bit missing");

        Assert.True(entries.ContainsKey("Kavita/sub/tool"));
        Assert.True(entries["Kavita/sub/tool"].Mode.HasFlag(UnixFileMode.GroupExecute), "nested executable bit missing");

        Assert.True(entries.ContainsKey("Kavita/appsettings.json"));
        Assert.False(entries["Kavita/appsettings.json"].Mode.HasFlag(UnixFileMode.UserExecute), "non-executable file got exec bits");

        Assert.True(entries.ContainsKey("Kavita/sub/"), "subdirectory entry missing");
    }

    [Fact]
    public async Task IncludeExternalFiles_CreatesMissingDestinationDirectories()
    {
        var outputPath = CreateOutputPath();
        var archiveDir = Path.Combine(_root, "out") + "/";

        var externalFiles = new Dictionary<string, List<ExternalFile>>
        {
            ["linux-x64"] =
            [
                new ExternalFile { From = "https://example.com/tool", To = "bin/deep/tool", Executable = true, Sha256 = "" }
            ]
        };

        var handler = new StubHandler();
        var stage = new BuildServerStage(NullLogger<BuildServerStage>.Instance, null!, new HttpClient(handler));
        var config = CreateConfig(archiveDir, externalFiles);

        var result =
            await stage.IncludeExternalFiles(CreateContext(), config, "linux-x64", outputPath, CancellationToken.None);

        var destPath = Path.Combine(outputPath, "bin", "deep", "tool");
        Assert.True(File.Exists(destPath), "external file was not written to nested destination");
        Assert.Contains("bin/deep/tool", result);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("binary")
            });
        }
    }
}
