
using System;

namespace GameClubManager.Data.Models;

public class GameSessions
{
    public int Id { get; set; }

    public int PlayerId { get; set; }

    public int GameId { get; set; }

    public DateTime StartedAt { get; set; }

    public int DurationMinutes { get; set; }

    public decimal? Deposit { get; set; }

    public Players Player { get; set; } = null!;

    public Games Game { get; set; } = null!;

    public override string ToString()
    {
        return $"{Player?.Nickname} — {Game?.Title}, " +
               $"{DurationMinutes} мин., депозит: {Deposit?.ToString("0.##") ?? "не указан"}";
    }
}
