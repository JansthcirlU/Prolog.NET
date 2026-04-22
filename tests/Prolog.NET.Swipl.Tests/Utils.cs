namespace Prolog.NET.Swipl.Tests;

internal static class Utils
{
    internal static IEnumerable<string> Which(string executableName)
    {
        string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (string dir in path.Split(Path.PathSeparator))
        {
            string fullPath = Path.Combine(dir, executableName);
            if (File.Exists(fullPath))
            {
                yield return fullPath;
            }
        }
    }
}