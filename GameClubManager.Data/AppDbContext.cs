
using GameClubManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GameClubManager.Data;

public class AppDbContext : DbContext
{
    public static string DbPath
    {
        get
        {
            var folder = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

            Directory.CreateDirectory(folder);

            return Path.Combine(folder, "gameclubmanager.db");
        }
    }

    public DbSet<Players> Players => Set<Players>();

    public DbSet<Games> Games => Set<Games>();

    public DbSet<GameSessions> GameSessions => Set<GameSessions>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}
