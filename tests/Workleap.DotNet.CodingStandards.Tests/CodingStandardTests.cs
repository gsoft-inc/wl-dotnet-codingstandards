using Workleap.DotNet.CodingStandards.Tests.Helpers;
using Xunit.Abstractions;

namespace Workleap.DotNet.CodingStandards.Tests;

public sealed class CodingStandardTests(PackageFixture fixture, ITestOutputHelper testOutputHelper) : IClassFixture<PackageFixture>
{
    [Fact]
    public async Task BannedSymbolsAreReported()
    {
        using var project = new ProjectBuilder(fixture, testOutputHelper);
        project.AddCsprojFile();
        project.AddFile("sample.cs", "_ = System.DateTime.Now;");
        var data = await project.BuildAndGetOutput();
        Assert.True(data.HasWarning("RS0030"));
    }

    [Fact]
    public async Task WarningsAsErrorOnGitHubActions()
    {
        using var project = new ProjectBuilder(fixture, testOutputHelper);
        project.AddCsprojFile();
        project.AddFile("sample.cs", "_ = System.DateTime.Now;");
        var data = await project.BuildAndGetOutput(["--configuration", "Release", "/p:GITHUB_ACTIONS=true"]);
        Assert.True(data.HasError("RS0030"));
    }

    [Fact]
    public async Task MSBuildWarningsAsErrorOnDebugConfiguration()
    {
        using var project = new ProjectBuilder(fixture, testOutputHelper);
        project.AddCsprojFile(packageReferences: new Dictionary<string, string> { { "Azure.Identity", "1.10.4" } });
        project.AddFile("sample.cs", """
             namespace sample;
             public static class Sample
             {
                 public static void Main(string[] args)
                 {
                 }
             }
             """);
        var data = await project.BuildAndGetOutput();
        Assert.True(data.HasWarning("NU1902"));
    }

    [Fact]
    public async Task MSBuildWarningsAsErrorOnReleaseConfiguration()
    {
        using var project = new ProjectBuilder(fixture, testOutputHelper);
        project.AddCsprojFile(packageReferences: new Dictionary<string, string> { { "Azure.Identity", "1.10.4" } });
        project.AddFile("sample.cs", """
             namespace sample;
             public static class Sample
             {
                 public static void Main(string[] args)
                 {
                 }
             }
             """);
        var data = await project.BuildAndGetOutput(["--configuration", "Release"]);
        Assert.True(data.HasError("NU1902"));
    }

    [Fact]
    public async Task NamingConvention_Invalid()
    {
        using var project = new ProjectBuilder(fixture, testOutputHelper);
        project.AddCsprojFile();
        project.AddFile("sample.cs", """
            _ = "";

            class Sample
            {
                private readonly int field;

                public Sample(int a) => field = a;

                public int A() => field;
            }
            """);
        var data = await project.BuildAndGetOutput(["--configuration", "Release"]);
        Assert.True(data.HasError("IDE1006"));
    }

    [Fact]
    public async Task NamingConvention_Valid()
    {
        using var project = new ProjectBuilder(fixture, testOutputHelper);
        project.AddCsprojFile();
        project.AddFile("sample.cs", """
            _ = "";

            class Sample
            {
                private int _field;
            }
            """);
        var data = await project.BuildAndGetOutput(["--configuration", "Release"]);
        Assert.False(data.HasError("IDE1006"));
        Assert.False(data.HasWarning("IDE1006"));
    }
}
