using System.IO;

namespace MazEdit.Tests;

internal static class GoldenMaz
{
    /// <summary>
    /// Real machine files stay gitignored. Set MAZEDIT_TEST_MAZ or copy TEST.MAZ into TestData/.
    /// </summary>
    public static string? TryFindTestMaz()
    {
        string? env = Environment.GetEnvironmentVariable("MAZEDIT_TEST_MAZ");
        if (!string.IsNullOrWhiteSpace(env) && File.Exists(env))
            return env;

        foreach (string root in EnumerateAncestors(AppContext.BaseDirectory))
        {
            foreach (string name in new[] { "TEST.MAZ", "TEST.maz", "Test.maz" })
            {
                string candidate = Path.Combine(root, "TestData", name);
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateAncestors(string start)
    {
        DirectoryInfo? dir = new(start);
        while (dir is not null)
        {
            yield return dir.FullName;
            dir = dir.Parent;
        }
    }
}
