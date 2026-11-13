
using GameClubManager.Data;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("==================================");
Console.WriteLine("       GAME CLUB MANAGER");
Console.WriteLine("==================================");

Console.WriteLine($"\nПуть к базе данных:\n{AppDbContext.DbPath}");

// Создаём подключение к базе
using var db = new AppDbContext();

// Создаём базу, если она ещё не существует
db.Database.EnsureCreated();

// Заполняем базу тестовыми данными
DbSeed.Seed(db);

Console.WriteLine("\n========== ИГРОКИ ==========");

var players = db.Players.ToList();

foreach (var player in players)
{
    Console.WriteLine(player);
}

Console.WriteLine("\n========== ИГРЫ ==========");

var games = db.Games.ToList();

foreach (var game in games)
{
    Console.WriteLine(game);
}

Console.WriteLine("\n======= ИГРОВЫЕ СЕССИИ =======");

var sessions = db.GameSessions
    .Include(s => s.Player)
    .Include(s => s.Game)
    .ToList();

foreach (var session in sessions)
{
    Console.WriteLine(session);
}

Console.WriteLine("\n==================================");
Console.WriteLine("Количество игроков: " + players.Count);
Console.WriteLine("Количество игр: " + games.Count);
Console.WriteLine("Количество сессий: " + sessions.Count);
Console.WriteLine("==================================");

Console.WriteLine("\nРабота программы завершена.");
