
using System.Data;
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

    // При обновлении старой базы меняю название столбца без потери записей.
    public void EnsureDepositColumn()
    {
        var connection = Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;

        if (wasClosed)
            connection.Open();

        bool hasScore = false;
        bool hasDeposit = false;

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA table_info('GameSessions')";
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var columnName = reader.GetString(1);
                hasScore |= columnName == "Score";
                hasDeposit |= columnName == "Deposit";
            }
        }

        if (wasClosed)
            connection.Close();

        if (hasScore && !hasDeposit)
            Database.ExecuteSqlRaw("ALTER TABLE GameSessions RENAME COLUMN Score TO Deposit");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}
