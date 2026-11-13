
using System;
using System.Collections.Generic;

namespace GameClubManager.Data.Models;

public class Players
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public DateTime RegistrationDate { get; set; }

    public string MembershipLevel { get; set; } = string.Empty;

    public List<GameSessions> GameSessions { get; set; } = new();

    public override string ToString()
    {
        return $"{Nickname} ({FullName}) — {MembershipLevel}";
    }
}
