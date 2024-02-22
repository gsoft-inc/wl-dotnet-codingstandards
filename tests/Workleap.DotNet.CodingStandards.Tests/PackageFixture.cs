using CliWrap;
using Meziantou.Framework;
using Workleap.DotNet.CodingStandards.Tests.Helpers;

namespace Workleap.DotNet.CodingStandards.Tests;

public sealed class PackageFixture : IAsyncLifetime
{
    private readonly TemporaryDirectory _packageDirectory = TemporaryDirectory.Create();

    public FullPath PackageDirectory => _packageDirectory.FullPath;

    public async Task InitializeAsync()
    {
        // On CI the exe is already present
        var exe = "nuget";
        if (OperatingSystem.IsWindows())
        {
            var downloadPath = FullPath.GetTempPath() / $"nuget-{Guid.NewGuid()}.exe";
            await DownloadFileAsync("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", downloadPath);
            exe = downloadPath;
        }

        var nuspecPath = PathHelpers.GetRootDirectory() / "Workleap.DotNet.CodingStandards.nuspec";
        await Cli.Wrap(exe)
            .WithArguments(["pack", nuspecPath, "-ForceEnglishOutput", "-Version", "999.9.9", "-OutputDirectory", _packageDirectory.FullPath])
            .ExecuteAsync();
    }

    public async Task DisposeAsync()
    {
        await _packageDirectory.DisposeAsync();
    }

    private static async Task DownloadFileAsync(string url, FullPath path)
    {
        path.CreateParentDirectory();
        await using var nugetStream = await SharedHttpClient.Instance.GetStreamAsync(url);
        await using var fileStream = File.Create(path);
        await nugetStream.CopyToAsync(fileStream);
    }
}
