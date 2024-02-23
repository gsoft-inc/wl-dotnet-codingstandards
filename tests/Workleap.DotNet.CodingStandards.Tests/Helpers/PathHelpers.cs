namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal static class PathHelpers
{
    public static string GetRootDirectory()
    {
        var directory = Environment.CurrentDirectory;
        while (!Directory.Exists(Path.Combine(directory, ".git")))
        {
            directory = Path.GetDirectoryName(directory);
        }

        return directory;
    }
}
