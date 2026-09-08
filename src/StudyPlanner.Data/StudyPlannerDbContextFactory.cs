using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudyPlanner.Data;

/// <summary>
/// فقط برای Design-Time (دستور "dotnet ef migrations add ..."). خودِ برنامه از
/// این کلاس استفاده نمی‌کند - App.xaml.cs مسیر واقعی %AppData% را پیکربندی می‌کند.
/// </summary>
public class StudyPlannerDbContextFactory : IDesignTimeDbContextFactory<StudyPlannerDbContext>
{
    public StudyPlannerDbContext CreateDbContext(string[] args)
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StudyPlannerApp");
        Directory.CreateDirectory(appDataFolder);
        var dbPath = Path.Combine(appDataFolder, "studyplanner.db");

        var optionsBuilder = new DbContextOptionsBuilder<StudyPlannerDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new StudyPlannerDbContext(optionsBuilder.Options);
    }
}
