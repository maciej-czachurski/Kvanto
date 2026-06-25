using Kvanto.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace Kvanto.Data;

public class KvantoDbContext : DbContext
{
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<PomodoroSession> PomodoroSessions { get; set; } = null!;
    public DbSet<AppSettings> Settings { get; set; } = null!;

    private static readonly string DatabasePath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kvanto",
            "kvanto.db");

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dir = Path.GetDirectoryName(DatabasePath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        options.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.HasMany(e => e.Sessions)
                  .WithOne(s => s.TaskItem)
                  .HasForeignKey(s => s.TaskItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PomodoroSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Seed default settings row
            entity.HasData(new AppSettings { Id = 1 });
        });
    }
}
