using System.IO;

namespace StudyPlanner.App;

public static class AppPaths
{
    public static string AppDataFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "StudyPlannerApp");

    public static string DbPath => Path.Combine(AppDataFolder, "studyplanner.db");

    public static void EnsureAppDataFolderExists() => Directory.CreateDirectory(AppDataFolder);
}
