namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal sealed class TemporaryDirectory : IDisposable
{
    private TemporaryDirectory(string fullPath) => this.FullPath = fullPath;

    public string FullPath { get; }

    public static TemporaryDirectory Create()
    {
        var path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        _ = Directory.CreateDirectory(path);
        return new TemporaryDirectory(path);
    }

    public string GetPath(string relativePath)
    {
        return Path.Combine(this.FullPath, relativePath);
    }

    public void CreateTextFile(string relativePath, string content)
    {
        var path = this.GetPath(relativePath);
        _ = Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(this.FullPath, recursive: true);
        }
        catch
        {
            // We use this code in tests, so it's not important if a folder cannot be deleted 
        }
    }
}
