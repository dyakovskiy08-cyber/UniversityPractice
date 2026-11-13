
using GameClubManager.Data.Models;

namespace GameClubManager.Data;

public static class DbSeed
{
    public static void Seed(AppDbContext db)
    {
        // Если база уже содержит записи, ничего не добавляем
        if (db.Players.Any() || db.Games.Any() || db.GameSessions.Any())
            return;

        // Добавляем игроков
        var players = new List<Players>
        {
            new Players
            {
                FullName = "Александр Иванов",
                Nickname = "Shadow",
                RegistrationDate = new DateTime(2026, 1, 15),
                MembershipLevel = "Gold"
            },
            new Players
            {
                FullName = "Дмитрий Смирнов",
                Nickname = "Dragon",
                RegistrationDate = new DateTime(2026, 2, 10),
                MembershipLevel = "Silver"
            },
            new Players
            {
                FullName = "Максим Петров",
                Nickname = "MaxPower",
                RegistrationDate = new DateTime(2026, 3, 5),
                MembershipLevel = "VIP"
            }
        };

        // Добавляем игры
        var games = new List<Games>
        {
            new Games
            {
                Title = "Cyberpunk 2077",
                Genre = "RPG",
                AgeRating = "18+",
                Developer = "CD Projekt Red"
            },
            new Games
            {
                Title = "Counter-Strike 2",
                Genre = "Shooter",
                AgeRating = "16+",
                Developer = "Valve"
            },
            new Games
            {
                Title = "The Witcher 3",
                Genre = "RPG",
                AgeRating = "18+",
                Developer = "CD Projekt Red"
            }
        };

        db.Players.AddRange(players);
        db.Games.AddRange(games);

        db.SaveChanges();

        // Добавляем игровые сессии
        var sessions = new List<GameSessions>
        {
            new GameSessions
            {
                PlayerId = players[0].Id,
                GameId = games[0].Id,
                StartedAt = new DateTime(2026, 11, 10, 15, 30, 0),
                DurationMinutes = 120,
                Score = 950
            },
            new GameSessions
            {
                PlayerId = players[1].Id,
                GameId = games[1].Id,
                StartedAt = new DateTime(2026, 11, 11, 17, 0, 0),
                DurationMinutes = 90,
                Score = 1200
            },
            new GameSessions
            {
                PlayerId = players[2].Id,
                GameId = games[2].Id,
                StartedAt = new DateTime(2026, 11, 12, 19, 15, 0),
                DurationMinutes = 150,
                Score = null
            },
            new GameSessions
            {
                PlayerId = players[0].Id,
                GameId = games[1].Id,
                StartedAt = new DateTime(2026, 11, 13, 12, 0, 0),
                DurationMinutes = 60,
                Score = 800
            }
        };

        db.GameSessions.AddRange(sessions);
        db.SaveChanges();
    }
}
