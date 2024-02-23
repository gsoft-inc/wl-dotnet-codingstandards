namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal static class PathHelpers
{
    public static string GetRootDirectory()
    {
        var directory = Environment.CurrentDirectory;
        while (directory != null && !Directory.Exists(Path.Combine(directory, ".git")))
        {
            directory = Path.GetDirectoryName(directory);
        }

        return directory ?? throw new InvalidOperationException("Cannot find the root of the git repository");
    }
}
