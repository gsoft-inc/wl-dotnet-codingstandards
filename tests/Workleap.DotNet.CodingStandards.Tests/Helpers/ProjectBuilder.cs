using System.Xml.Linq;
using Xunit.Abstractions;
using System.Text.Json;
using CliWrap;

namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal sealed class ProjectBuilder : IDisposable
{
    private const string SarifFileName = "BuildOutput.sarif";

    private readonly TemporaryDirectory _directory;
    private readonly ITestOutputHelper _testOutputHelper;

    public ProjectBuilder(PackageFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _directory = TemporaryDirectory.Create();
        _directory.CreateTextFile("NuGet.config", $"""
                <configuration>
                  <config>
                    <add key="globalPackagesFolder" value="{fixture.PackageDirectory}/packages" />
                  </config>
                  <packageSources>
                    <clear />
                    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
                    <add key="TestSource" value="{fixture.PackageDirectory}" />
                  </packageSources>
                  <packageSourceMapping>
                    <packageSource key="nuget.org">
                        <package pattern="*" />
                    </packageSource>
                    <packageSource key="TestSource">
                        <package pattern="Workleap.DotNet.CodingStandards" />
                    </packageSource>
                  </packageSourceMapping>
                </configuration>
                """);

        File.Copy(Path.Combine(PathHelpers.GetRootDirectory(), "global.json"), _directory.GetPath("global.json"));
    }

    public ProjectBuilder AddFile(string relativePath, string content)
    {
        File.WriteAllText(_directory.GetPath(relativePath), content);
        return this;
    }

    public ProjectBuilder AddCsprojFile(Dictionary<string, string> properties = null)
    {
        var element = new XElement("PropertyGroup");
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                element.Add(new XElement(prop.Key), prop.Value);
            }
        }

        var content = $"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <OutputType>exe</OutputType>
                    <TargetFramework>net$(NETCoreAppMaximumVersion)</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                    <ErrorLog>{SarifFileName},version=2.1</ErrorLog>
                  </PropertyGroup>
                  {element}

                  <ItemGroup>
                    <PackageReference Include="Workleap.DotNet.CodingStandards" Version="*" />
                  </ItemGroup>
                </Project>
                """;

        File.WriteAllText(_directory.GetPath("test.csproj"), content);
        return this;
    }

    public async Task<SarifFile> BuildAndGetOutput(string[] buildArguments = null)
    {
        var result = await Cli.Wrap("dotnet")
            .WithWorkingDirectory(_directory.FullPath)
            .WithArguments(["build", .. (buildArguments ?? [])])
            .WithEnvironmentVariables(env => env.Set("CI", null).Set("GITHUB_ACTIONS", null))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(_testOutputHelper.WriteLine))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(_testOutputHelper.WriteLine))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        _testOutputHelper.WriteLine("Process exit code: " + result.ExitCode);

        var bytes = File.ReadAllBytes(_directory.GetPath(SarifFileName));
        var sarif = JsonSerializer.Deserialize<SarifFile>(bytes);
        _testOutputHelper.WriteLine("Sarif result:\n" + string.Join("\n", sarif.AllResults().Select(r => r.ToString())));
        return sarif;
    }

    public void Dispose() => _directory.Dispose();
}
