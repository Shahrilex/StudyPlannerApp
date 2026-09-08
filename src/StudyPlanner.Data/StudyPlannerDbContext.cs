using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Models;

namespace StudyPlanner.Data;

public class StudyPlannerDbContext : DbContext
{
    public DbSet<StudyDay> StudyDays => Set<StudyDay>();
    public DbSet<StudyTopic> StudyTopics => Set<StudyTopic>();
    public DbSet<StudySession> StudySessions => Set<StudySession>();

    public StudyPlannerDbContext(DbContextOptions<StudyPlannerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudyDay>(entity =>
        {
            entity.HasIndex(d => d.GregorianDate).IsUnique();

            entity.HasMany(d => d.Topics)
                  .WithOne(t => t.StudyDay)
                  .HasForeignKey(t => t.StudyDayId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Sessions)
                  .WithOne(s => s.StudyDay)
                  .HasForeignKey(s => s.StudyDayId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudyTopic>(entity =>
        {
            entity.Property(t => t.Subject).HasConversion<int>();
            entity.Property(t => t.Priority).HasConversion<int>();
            entity.Property(t => t.Mastery).HasConversion<int>();
        });

        modelBuilder.Entity<StudySession>(entity =>
        {
            // مقادیر محاسبه‌شده - ستون واقعی در دیتابیس ندارند.
            entity.Ignore(s => s.ReviewDate1);
            entity.Ignore(s => s.ReviewDate2);
            entity.Ignore(s => s.ReviewDate3);
        });

        base.OnModelCreating(modelBuilder);
    }
}
