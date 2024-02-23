using CliWrap;
using CliWrap.Buffered;
using Workleap.DotNet.CodingStandards.Tests.Helpers;

namespace Workleap.DotNet.CodingStandards.Tests;

public sealed class PackageFixture : IAsyncLifetime
{
    private readonly TemporaryDirectory _packageDirectory = TemporaryDirectory.Create();

    public string PackageDirectory => this._packageDirectory.FullPath;

    public async Task InitializeAsync()
    {
        var nuspecPath = Path.Combine(PathHelpers.GetRootDirectory(), "Workleap.DotNet.CodingStandards.nuspec");
        string[] args = ["pack", nuspecPath, "-ForceEnglishOutput", "-Version", "999.9.9", "-OutputDirectory", this._packageDirectory.FullPath];

        if (OperatingSystem.IsWindows())
        {
            var exe = Path.Combine(Path.GetTempPath(), $"nuget-{Guid.NewGuid()}.exe");
            await DownloadFileAsync("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", exe);

            _ = await Cli.Wrap(exe)
                .WithArguments(args)
                .ExecuteAsync();
        }
        else
        {
            _ = await Cli.Wrap("nuget")
                 .WithArguments(args)
                 .ExecuteBufferedAsync();
        }
    }

    public Task DisposeAsync()
    {
        this._packageDirectory.Dispose();
        return Task.CompletedTask;
    }

    private static async Task DownloadFileAsync(string url, string path)
    {
        _ = Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var nugetStream = await SharedHttpClient.Instance.GetStreamAsync(url);
        await using var fileStream = File.Create(path);
        await nugetStream.CopyToAsync(fileStream);
    }
}
