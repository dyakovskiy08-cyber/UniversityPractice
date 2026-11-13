
using System.Collections.Generic;

namespace GameClubManager.Data.Models;

public class Games
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public string AgeRating { get; set; } = string.Empty;

    public string Developer { get; set; } = string.Empty;

    public List<GameSessions> GameSessions { get; set; } = new();

    public override string ToString()
    {
        return $"{Title} — {Genre}, {AgeRating} (разработчик: {Developer})";
    }
}
