using System.Diagnostics;
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
        var nuspecPath = PathHelpers.GetRootDirectory() / "Workleap.DotNet.CodingStandards.nuspec";
        string[] args = ["pack", nuspecPath, "-ForceEnglishOutput", "-Version", "999.9.9", "-OutputDirectory", _packageDirectory.FullPath];

        if (OperatingSystem.IsWindows())
        {
            var exe = FullPath.GetTempPath() / $"nuget-{Guid.NewGuid()}.exe";
            await DownloadFileAsync("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", exe);

            await Cli.Wrap(exe)
                .WithArguments(args)
                .ExecuteAsync();
        }
        else
        {
            // CliWrap doesn't support UseShellExecute. On Linux, it's easier to use it as "nuget" is a shell script that use mono to run nuget.exe
            var psi = new ProcessStartInfo("nuget");
            foreach (var arg in args)
            {
                psi.ArgumentList.Add(arg);
            }

            var p = Process.Start(psi);
            await p.WaitForExitAsync();
            if (p.ExitCode != 0)
                throw new InvalidOperationException("Error when running creating the NuGet package");
        }
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
