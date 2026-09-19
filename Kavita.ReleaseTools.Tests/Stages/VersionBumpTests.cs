using System.IO.Abstractions.TestingHelpers;
using Kavita.ReleaseTools.Models;
using Kavita.ReleaseTools.Stages.VersionBump;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Tests.Stages;

public class VersionBumpTests
{

    private const string CsprojPath = @"C:\proj\App.csproj";

    private static string Csproj(string version) =>
        $"""
         <Project Sdk="Microsoft.NET.Sdk">
           <PropertyGroup>
             <TargetFramework>net8.0</TargetFramework>
             <AssemblyVersion>{version}</AssemblyVersion>
           </PropertyGroup>
         </Project>
         """;

    private static (VersionBumpStage stage, ExecutionContext ctx, MockFileSystem fs) Arrange(string version, VersionComponent component, bool reset)
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [CsprojPath] = new(Csproj(version))
        });

        var ctx = StageTestsHelper.CreateExecutionContext(new ReleaseConfiguration
        {
            VersionBump = new VersionBumpConfiguration
            {
                Disabled = false,
                ComponentToBump = component,
                ResetSmallerComponents = reset,
                CsprojPath = CsprojPath,
            },
            GitData = new GitData(),
        }, fs);

        return (new VersionBumpStage(Substitute.For<ILogger<VersionBumpStage>>()), ctx, fs);
    }

    public static TheoryData<string, VersionComponent, bool, string> CsprojBumpTestData => new()
    {
        // Input   Component                        Reset  Expected AssemblyVersion
        { "1.2.3",    VersionComponent.Major,       false, "2.2.3"    },
        { "1.2.3",    VersionComponent.Major,       true,  "2.0.0"    },
        { "1.2.3.4",  VersionComponent.Major,       true,  "2.0.0.0"  },

        { "1.2.3",    VersionComponent.Minor,       false, "1.3.3"    },
        { "1.2.3",    VersionComponent.Minor,       true,  "1.3.0"    },
        { "1.2.3.4",  VersionComponent.Minor,       true,  "1.3.0.0"  },

        { "1.2.3",    VersionComponent.Build,       false, "1.2.4"    },
        { "1.2.3.4",  VersionComponent.Build,       true,  "1.2.4.0"  },

        { "1.1.0.2",  VersionComponent.Revision,    false, "1.1.0.3"  },
        { "1.2.3",    VersionComponent.Revision,    false, "1.2.3.1"  },
    };

    [Theory]
    [MemberData(nameof(CsprojBumpTestData))]
    public async Task Execute_BumpsAssemblyVersionInCsproj(string version, VersionComponent component, bool reset, string expected)
    {
        var (stage, ctx, fs) = Arrange(version, component, reset);

        await stage.ExecuteAsync(ctx, CancellationToken.None);

        var updated = await fs.File.ReadAllTextAsync(CsprojPath);
        Assert.Contains($"<AssemblyVersion>{expected}</AssemblyVersion>", updated);
    }

    [Fact]
    public async Task Execute_PreservesSurroundingCsprojContent()
    {
        var (stage, ctx, fs) = Arrange("1.2.3", VersionComponent.Minor, reset: true);

        await stage.ExecuteAsync(ctx, CancellationToken.None);

        var updated = await fs.File.ReadAllTextAsync(CsprojPath);
        Assert.Contains("<TargetFramework>net8.0</TargetFramework>", updated);
        Assert.StartsWith("<Project", updated.TrimStart());
        Assert.EndsWith("</Project>", updated.TrimEnd());
    }

    [Fact]
    public async Task Execute_Throws_WhenAssemblyVersionMissing()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [CsprojPath] = new("<Project><PropertyGroup /></Project>")
        });
        var ctx = StageTestsHelper.CreateExecutionContext(new ReleaseConfiguration
        {
            VersionBump = new VersionBumpConfiguration
            {
                Disabled = false,
                ComponentToBump = VersionComponent.Major,
                ResetSmallerComponents = true,
                CsprojPath = CsprojPath,
            },
            GitData = new GitData(),
        }, fs);

        await Assert.ThrowsAsync<ExecutionException>(() => new VersionBumpStage(Substitute.For<ILogger<VersionBumpStage>>()).ExecuteAsync(ctx, CancellationToken.None));
    }

    [Fact]
    public async Task Execute_Throws_WhenMultipleAssemblyVersionElements()
    {
        const string content = """
                               <Project>
                                 <PropertyGroup>
                                   <AssemblyVersion>1.2.3</AssemblyVersion>
                                   <AssemblyVersion>4.5.6</AssemblyVersion>
                                 </PropertyGroup>
                               </Project>
                               """;

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [CsprojPath] = new(content)
        });
        var ctx = StageTestsHelper.CreateExecutionContext(new ReleaseConfiguration
        {
            VersionBump = new VersionBumpConfiguration
            {
                Disabled = false,
                ComponentToBump = VersionComponent.Major,
                ResetSmallerComponents = true,
                CsprojPath = CsprojPath,
            },
            GitData = new GitData(),
        }, fs);

        await Assert.ThrowsAsync<ExecutionException>(() => new VersionBumpStage(Substitute.For<ILogger<VersionBumpStage>>()).ExecuteAsync(ctx, CancellationToken.None));
    }

    #region VersionBump (raw)

    public static TheoryData<Version, VersionComponent, bool, Version> VersionBumpBehaviorTestData => new()
    {
        { new Version(1, 2, 3),    VersionComponent.Major, false, new Version(2, 2, 3)    },
        { new Version(1, 2, 3),    VersionComponent.Major, true,  new Version(2, 0, 0)    },
        { new Version(1, 2, 3, 4), VersionComponent.Major, false, new Version(2, 2, 3, 4) },
        { new Version(1, 2, 3, 4), VersionComponent.Major, true,  new Version(2, 0, 0, 0) },
        { new Version(0, 0, 0),    VersionComponent.Major, true,  new Version(1, 0, 0)    },
        { new Version(1, 0, 0),    VersionComponent.Major, false, new Version(2, 0, 0)    },

        { new Version(1, 2, 3),    VersionComponent.Minor, false, new Version(1, 3, 3)    },
        { new Version(1, 2, 3),    VersionComponent.Minor, true,  new Version(1, 3, 0)    },
        { new Version(1, 2, 3, 4), VersionComponent.Minor, false, new Version(1, 3, 3, 4) },
        { new Version(1, 2, 3, 4), VersionComponent.Minor, true,  new Version(1, 3, 0, 0) },
        { new Version(1, 0, 0),    VersionComponent.Minor, false, new Version(1, 1, 0)    },

        { new Version(1, 2, 3),    VersionComponent.Build, false, new Version(1, 2, 4)    },
        { new Version(1, 2, 3, 4), VersionComponent.Build, false, new Version(1, 2, 4, 4) },
        { new Version(1, 2, 3, 4), VersionComponent.Build, true,  new Version(1, 2, 4, 0) },
        { new Version(1, 2, 0),    VersionComponent.Build, false, new Version(1, 2, 1)    },

        { new Version(1, 1, 0, 2), VersionComponent.Revision, false, new Version(1, 1, 0, 3) },
        { new Version(1, 1, 0, 2), VersionComponent.Revision, true,  new Version(1, 1, 0, 3) },
        { new Version(1, 1, 0, 0), VersionComponent.Revision, false, new Version(1, 1, 0, 1) },
        { new Version(1, 1, 0, 9), VersionComponent.Revision, false, new Version(1, 1, 0, 10) },
        { new Version(1, 2, 3),    VersionComponent.Revision, false, new Version(1, 2, 3, 1) },
        { new Version(1, 2, 3),    VersionComponent.Revision, true,  new Version(1, 2, 3, 1) },
    };

    [Theory]
    [MemberData(nameof(VersionBumpBehaviorTestData))]
    public void Test_VersionBumpBehavior(Version version, VersionComponent component, bool resetSmallerComponents, Version expected)
    {
        var actual = VersionBumpStage.BumpVersion(new VersionBumpConfiguration
        {
            Disabled = false,
            ComponentToBump = component,
            ResetSmallerComponents = resetSmallerComponents,
            CsprojPath = string.Empty
        }, version);

        Assert.Equal(expected, actual);
    }

    #endregion

}
