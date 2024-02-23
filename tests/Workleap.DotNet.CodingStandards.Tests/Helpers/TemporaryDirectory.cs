namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal sealed class TemporaryDirectory : IDisposable
{
    private TemporaryDirectory(string fullPath) => FullPath = fullPath;

    public string FullPath { get; }

    public static TemporaryDirectory Create()
    {
        var path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(path);
        return new TemporaryDirectory(path);
    }

    public string GetPath(string relativePath)
    {
        return Path.Combine(FullPath, relativePath);
    }

    public void CreateTextFile(string relativePath, string content)
    {
        var path = GetPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(FullPath, recursive: true);
        }
        catch
        {
            // We use this code in tests, so it's not important if a folder cannot be deleted 
        }
    }
}
