using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GameClubManager.Data;
using GameClubManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace UniversityPractice.Views;

public partial class MainView : UserControl
{
    private int? _editingPlayerId;
    private int? _editingGameId;
    private int? _editingSessionId;

    public MainView()
    {
        InitializeComponent();

        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            DbSeed.Seed(db);
        }

        LoadPlayers();
        LoadGames();
        LoadSessions();

        GamesList.SelectionChanged += (_, _) =>
        {
            if (GamesList.SelectedItem is Games game)
            {
                _editingGameId = game.Id;
                GameTitleBox.Text = game.Title;
                GameGenreBox.Text = game.Genre;
                GameDeveloperBox.Text = game.Developer;
                GameAgeRatingBox.Text = game.AgeRating;
                GameStatus.Text = $"Редактирование игры: {game.Title}";
            }
        };

        SessionsList.SelectionChanged += (_, _) =>
        {
            if (SessionsList.SelectedItem is GameSessions session)
            {
                _editingSessionId = session.Id;

                SessionPlayerCombo.SelectedItem =
                    (SessionPlayerCombo.ItemsSource as System.Collections.IEnumerable)?
                    .Cast<Players>()
                    .FirstOrDefault(p => p.Id == session.PlayerId);

                SessionGameCombo.SelectedItem =
                    (SessionGameCombo.ItemsSource as System.Collections.IEnumerable)?
                    .Cast<Games>()
                    .FirstOrDefault(g => g.Id == session.GameId);

                SessionStartedAtBox.Text =
                    session.StartedAt.ToString("yyyy-MM-dd HH:mm");

                SessionDurationBox.Text =
                    session.DurationMinutes.ToString();

                SessionScoreBox.Text =
                    session.Score?.ToString() ?? string.Empty;

                SessionStatus.Text = $"Редактирование сеанса №{session.Id}";
            }
        };
    }

    // ЗАГРУЗКА ДАННЫХ

    private void LoadPlayers()
    {
        using var db = new AppDbContext();

        var players = db.Players
            .OrderBy(p => p.FullName)
            .ToList();

        PlayersList.ItemsSource = players;
        SessionPlayerCombo.ItemsSource = players;
    }

    private void LoadGames()
    {
        using var db = new AppDbContext();

        var games = db.Games
            .OrderBy(g => g.Title)
            .ToList();

        GamesList.ItemsSource = games;
        SessionGameCombo.ItemsSource = games;
    }

    private void LoadSessions()
    {
        using var db = new AppDbContext();

        SessionsList.ItemsSource = db.GameSessions
            .Include(s => s.Player)
            .Include(s => s.Game)
            .OrderByDescending(s => s.StartedAt)
            .ToList();
    }

    // СВЯЗАННЫЕ СЕАНСЫ ВЫБРАННОГО ИГРОКА

    private void PlayersList_SelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (PlayersList.SelectedItem is not Players player)
        {
            PlayerSessionsList.ItemsSource = null;
            return;
        }

        _editingPlayerId = player.Id;
        FullNameBox.Text = player.FullName;
        NicknameBox.Text = player.Nickname;
        MembershipLevelBox.Text = player.MembershipLevel;
        PlayerStatus.Text = $"Редактирование игрока: {player.Nickname}";

        LoadPlayerSessions(player.Id);
    }

    private void LoadPlayerSessions(int playerId)
    {
        using var db = new AppDbContext();

        PlayerSessionsList.ItemsSource = db.GameSessions
            .Include(s => s.Player)
            .Include(s => s.Game)
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.StartedAt)
            .ToList();
    }

    // ИГРОКИ

    private void NewPlayer_Click(object? sender, RoutedEventArgs e)
    {
        _editingPlayerId = null;
        PlayersList.SelectedItem = null;

        FullNameBox.Text = "";
        NicknameBox.Text = "";
        MembershipLevelBox.Text = "";

        PlayerStatus.Text = "Заполнение данных нового игрока.";
    }

    private void SavePlayer_Click(object? sender, RoutedEventArgs e)
    {
        string fullName = FullNameBox.Text?.Trim() ?? "";
        string nickname = NicknameBox.Text?.Trim() ?? "";
        string membership = MembershipLevelBox.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(nickname) ||
            string.IsNullOrWhiteSpace(membership))
        {
            PlayerStatus.Text = "Заполните все поля.";
            return;
        }

        using var db = new AppDbContext();

        if (_editingPlayerId.HasValue)
        {
            var player = db.Players.FirstOrDefault(
                p => p.Id == _editingPlayerId.Value);

            if (player == null)
            {
                PlayerStatus.Text = "Игрок не найден.";
                return;
            }

            player.FullName = fullName;
            player.Nickname = nickname;
            player.MembershipLevel = membership;
        }
        else
        {
            db.Players.Add(new Players
            {
                FullName = fullName,
                Nickname = nickname,
                MembershipLevel = membership,
                RegistrationDate = DateTime.Now
            });
        }

        db.SaveChanges();

        _editingPlayerId = null;
        LoadPlayers();

        PlayersList.SelectedItem = null;
        FullNameBox.Text = "";
        NicknameBox.Text = "";
        MembershipLevelBox.Text = "";

        PlayerStatus.Text = "Данные игрока сохранены.";
    }

    private void DeletePlayer_Click(object? sender, RoutedEventArgs e)
    {
        if (PlayersList.SelectedItem is not Players selected)
        {
            PlayerStatus.Text = "Сначала выберите игрока.";
            return;
        }

        using var db = new AppDbContext();

        if (db.GameSessions.Any(s => s.PlayerId == selected.Id))
        {
            PlayerStatus.Text = "Нельзя удалить игрока: у него есть игровые сеансы.";
            return;
        }

        var player = db.Players.FirstOrDefault(p => p.Id == selected.Id);

        if (player == null)
        {
            PlayerStatus.Text = "Игрок не найден.";
            return;
        }

        db.Players.Remove(player);
        db.SaveChanges();

        NewPlayer_Click(null, e);
        LoadPlayers();

        PlayerStatus.Text = "Игрок удалён.";
    }

    // ИГРЫ

    private void NewGame_Click(object? sender, RoutedEventArgs e)
    {
        _editingGameId = null;
        GamesList.SelectedItem = null;

        GameTitleBox.Text = "";
        GameGenreBox.Text = "";
        GameDeveloperBox.Text = "";
        GameAgeRatingBox.Text = "";

        GameStatus.Text = "Заполнение данных новой игры.";
    }

    private void SaveGame_Click(object? sender, RoutedEventArgs e)
    {
        string title = GameTitleBox.Text?.Trim() ?? "";
        string genre = GameGenreBox.Text?.Trim() ?? "";
        string developer = GameDeveloperBox.Text?.Trim() ?? "";
        string ageRating = GameAgeRatingBox.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(genre) ||
            string.IsNullOrWhiteSpace(developer) ||
            string.IsNullOrWhiteSpace(ageRating))
        {
            GameStatus.Text = "Заполните все поля.";
            return;
        }

        using var db = new AppDbContext();

        if (_editingGameId.HasValue)
        {
            var game = db.Games.FirstOrDefault(
                g => g.Id == _editingGameId.Value);

            if (game == null)
            {
                GameStatus.Text = "Игра не найдена.";
                return;
            }

            game.Title = title;
            game.Genre = genre;
            game.Developer = developer;
            game.AgeRating = ageRating;
        }
        else
        {
            db.Games.Add(new Games
            {
                Title = title,
                Genre = genre,
                Developer = developer,
                AgeRating = ageRating
            });
        }

        db.SaveChanges();

        _editingGameId = null;
        LoadGames();

        GamesList.SelectedItem = null;
        GameTitleBox.Text = "";
        GameGenreBox.Text = "";
        GameDeveloperBox.Text = "";
        GameAgeRatingBox.Text = "";

        GameStatus.Text = "Данные игры сохранены.";
    }

    private void DeleteGame_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is not Games selected)
        {
            GameStatus.Text = "Сначала выберите игру.";
            return;
        }

        using var db = new AppDbContext();

        if (db.GameSessions.Any(s => s.GameId == selected.Id))
        {
            GameStatus.Text = "Нельзя удалить игру: с ней связаны игровые сеансы.";
            return;
        }

        var game = db.Games.FirstOrDefault(g => g.Id == selected.Id);

        if (game == null)
        {
            GameStatus.Text = "Игра не найдена.";
            return;
        }

        db.Games.Remove(game);
        db.SaveChanges();

        NewGame_Click(null, e);
        LoadGames();

        GameStatus.Text = "Игра удалена.";
    }

    // ИГРОВЫЕ СЕАНСЫ

    private void NewSession_Click(object? sender, RoutedEventArgs e)
    {
        _editingSessionId = null;
        SessionsList.SelectedItem = null;

        SessionPlayerCombo.SelectedItem = null;
        SessionGameCombo.SelectedItem = null;

        SessionStartedAtBox.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        SessionDurationBox.Text = "";
        SessionScoreBox.Text = "";

        SessionStatus.Text = "Заполнение данных нового сеанса.";
    }

    private void SaveSession_Click(object? sender, RoutedEventArgs e)
    {
        if (SessionPlayerCombo.SelectedItem is not Players player)
        {
            SessionStatus.Text = "Выберите игрока.";
            return;
        }

        if (SessionGameCombo.SelectedItem is not Games game)
        {
            SessionStatus.Text = "Выберите игру.";
            return;
        }

        if (!DateTime.TryParseExact(
                SessionStartedAtBox.Text?.Trim(),
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime startedAt))
        {
            SessionStatus.Text = "Введите дату в формате гггг-ММ-дд ЧЧ:мм.";
            return;
        }

        if (!int.TryParse(SessionDurationBox.Text, out int duration) ||
            duration <= 0)
        {
            SessionStatus.Text = "Длительность должна быть положительным числом.";
            return;
        }

        int? score = null;

        if (!string.IsNullOrWhiteSpace(SessionScoreBox.Text))
        {
            if (!int.TryParse(SessionScoreBox.Text, out int parsedScore) ||
                parsedScore < 0)
            {
                SessionStatus.Text = "Очки должны быть целым неотрицательным числом.";
                return;
            }

            score = parsedScore;
        }

        using var db = new AppDbContext();

        if (_editingSessionId.HasValue)
        {
            var session = db.GameSessions.FirstOrDefault(
                s => s.Id == _editingSessionId.Value);

            if (session == null)
            {
                SessionStatus.Text = "Сеанс не найден.";
                return;
            }

            session.PlayerId = player.Id;
            session.GameId = game.Id;
            session.StartedAt = startedAt;
            session.DurationMinutes = duration;
            session.Score = score;
        }
        else
        {
            db.GameSessions.Add(new GameSessions
            {
                PlayerId = player.Id,
                GameId = game.Id,
                StartedAt = startedAt,
                DurationMinutes = duration,
                Score = score
            });
        }

        db.SaveChanges();

        _editingSessionId = null;

        LoadSessions();

        if (PlayersList.SelectedItem is Players selectedPlayer)
        {
            LoadPlayerSessions(selectedPlayer.Id);
        }

        SessionsList.SelectedItem = null;
        SessionPlayerCombo.SelectedItem = null;
        SessionGameCombo.SelectedItem = null;
        SessionStartedAtBox.Text = "";
        SessionDurationBox.Text = "";
        SessionScoreBox.Text = "";

        SessionStatus.Text = "Игровой сеанс сохранён.";
    }

    private void DeleteSession_Click(object? sender, RoutedEventArgs e)
    {
        if (SessionsList.SelectedItem is not GameSessions selected)
        {
            SessionStatus.Text = "Сначала выберите игровой сеанс.";
            return;
        }

        using var db = new AppDbContext();

        var session = db.GameSessions.FirstOrDefault(
            s => s.Id == selected.Id);

        if (session == null)
        {
            SessionStatus.Text = "Сеанс не найден.";
            return;
        }

        db.GameSessions.Remove(session);
        db.SaveChanges();

        NewSession_Click(null, e);
        LoadSessions();

        if (PlayersList.SelectedItem is Players selectedPlayer)
        {
            LoadPlayerSessions(selectedPlayer.Id);
        }

        SessionStatus.Text = "Игровой сеанс удалён.";
    }
}