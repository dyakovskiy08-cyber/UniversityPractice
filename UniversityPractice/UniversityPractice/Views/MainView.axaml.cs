using System;
using System.Linq;
using System.Globalization;
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
            db.EnsureDepositColumn();
            DbSeed.Seed(db);
        }

        SessionDatePicker.SelectedDate = DateTimeOffset.Now;
        SessionTimePicker.SelectedTime = DateTime.Now.TimeOfDay;

        LoadPlayers();
        LoadGames();
        LoadSessions();
    }

    private void LoadPlayers()
    {
        using var db = new AppDbContext();
        var players = db.Players.OrderBy(p => p.FullName).ToList();
        PlayersList.ItemsSource = players;
        SessionPlayerCombo.ItemsSource = players;
    }

    private void LoadGames()
    {
        using var db = new AppDbContext();
        var games = db.Games.OrderBy(g => g.Title).ToList();
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

    private void LoadPlayerSessions(int playerId)
    {
        using var db = new AppDbContext();
        PlayerSessionsList.ItemsSource = db.GameSessions
            .Include(s => s.Game)
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.StartedAt)
            .ToList();
    }

    private void PlayersList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
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

    private void GamesList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesList.SelectedItem is not Games game)
            return;

        _editingGameId = game.Id;
        GameTitleBox.Text = game.Title;
        GameGenreBox.Text = game.Genre;
        GameDeveloperBox.Text = game.Developer;
        GameAgeRatingBox.Text = game.AgeRating;
        GameStatus.Text = $"Редактирование игры: {game.Title}";
    }

    private void SessionsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SessionsList.SelectedItem is not GameSessions session)
            return;

        _editingSessionId = session.Id;
        SessionPlayerCombo.SelectedItem =
            (SessionPlayerCombo.ItemsSource as System.Collections.IEnumerable)?
            .Cast<Players>().FirstOrDefault(p => p.Id == session.PlayerId);
        SessionGameCombo.SelectedItem =
            (SessionGameCombo.ItemsSource as System.Collections.IEnumerable)?
            .Cast<Games>().FirstOrDefault(g => g.Id == session.GameId);
        SessionDatePicker.SelectedDate = new DateTimeOffset(session.StartedAt.Date);
        SessionTimePicker.SelectedTime = session.StartedAt.TimeOfDay;
        SessionDurationBox.Text = session.DurationMinutes.ToString();
        SessionDepositBox.Text = session.Deposit?.ToString() ?? "";
        SessionStatus.Text = $"Редактирование сеанса №{session.Id}";
    }

    private void NewPlayer_Click(object? sender, RoutedEventArgs e)
    {
        _editingPlayerId = null;
        PlayersList.SelectedItem = null;
        FullNameBox.Text = "";
        NicknameBox.Text = "";
        MembershipLevelBox.Text = "";
        PlayerStatus.Text = "Добавление нового игрока.";
    }

    private void SavePlayer_Click(object? sender, RoutedEventArgs e)
    {
        var fullName = FullNameBox.Text?.Trim() ?? "";
        var nickname = NicknameBox.Text?.Trim() ?? "";
        var membership = MembershipLevelBox.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(nickname) ||
            string.IsNullOrWhiteSpace(membership))
        {
            PlayerStatus.Text = "Заполните все поля.";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            if (_editingPlayerId.HasValue)
            {
                var player = db.Players.FirstOrDefault(p => p.Id == _editingPlayerId.Value);
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
            LoadSessions();
            PlayersList.SelectedItem = null;
            FullNameBox.Text = NicknameBox.Text = MembershipLevelBox.Text = "";
            PlayerStatus.Text = "Игрок сохранён.";
        }
        catch (Exception ex)
        {
            PlayerStatus.Text = $"Ошибка сохранения: {ex.Message}";
        }
    }

    private void DeletePlayer_Click(object? sender, RoutedEventArgs e)
    {
        if (PlayersList.SelectedItem is not Players selected)
        {
            PlayerStatus.Text = "Сначала выберите игрока.";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            if (db.GameSessions.Any(s => s.PlayerId == selected.Id))
            {
                PlayerStatus.Text = "Нельзя удалить игрока: сначала удалите его игровые сеансы.";
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
        catch (Exception ex)
        {
            PlayerStatus.Text = $"Ошибка удаления: {ex.Message}";
        }
    }

    private void NewGame_Click(object? sender, RoutedEventArgs e)
    {
        _editingGameId = null;
        GamesList.SelectedItem = null;
        GameTitleBox.Text = GameGenreBox.Text = GameDeveloperBox.Text = GameAgeRatingBox.Text = "";
        GameStatus.Text = "Добавление новой игры.";
    }

    private void SaveGame_Click(object? sender, RoutedEventArgs e)
    {
        var title = GameTitleBox.Text?.Trim() ?? "";
        var genre = GameGenreBox.Text?.Trim() ?? "";
        var developer = GameDeveloperBox.Text?.Trim() ?? "";
        var ageRating = GameAgeRatingBox.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(genre) ||
            string.IsNullOrWhiteSpace(developer) || string.IsNullOrWhiteSpace(ageRating))
        {
            GameStatus.Text = "Заполните все поля.";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            if (_editingGameId.HasValue)
            {
                var game = db.Games.FirstOrDefault(g => g.Id == _editingGameId.Value);
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
            LoadSessions();
            GamesList.SelectedItem = null;
            GameTitleBox.Text = GameGenreBox.Text = GameDeveloperBox.Text = GameAgeRatingBox.Text = "";
            GameStatus.Text = "Игра сохранена.";
        }
        catch (Exception ex)
        {
            GameStatus.Text = $"Ошибка сохранения: {ex.Message}";
        }
    }

    private void DeleteGame_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is not Games selected)
        {
            GameStatus.Text = "Сначала выберите игру.";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            if (db.GameSessions.Any(s => s.GameId == selected.Id))
            {
                GameStatus.Text = "Нельзя удалить игру: сначала удалите связанные сеансы.";
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
        catch (Exception ex)
        {
            GameStatus.Text = $"Ошибка удаления: {ex.Message}";
        }
    }

    private void NewSession_Click(object? sender, RoutedEventArgs e)
    {
        _editingSessionId = null;
        SessionsList.SelectedItem = null;
        SessionPlayerCombo.SelectedItem = null;
        SessionGameCombo.SelectedItem = null;
        SessionDatePicker.SelectedDate = DateTimeOffset.Now;
        SessionTimePicker.SelectedTime = DateTime.Now.TimeOfDay;
        SessionDurationBox.Text = "";
        SessionDepositBox.Text = "";
        SessionStatus.Text = "Добавление нового сеанса.";
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

        if (SessionDatePicker.SelectedDate is not DateTimeOffset date)
        {
            SessionStatus.Text = "Выберите дату сеанса.";
            return;
        }

        if (SessionTimePicker.SelectedTime is not TimeSpan time)
        {
            SessionStatus.Text = "Выберите время сеанса.";
            return;
        }

        if (!int.TryParse(SessionDurationBox.Text, out var duration) || duration <= 0)
        {
            SessionStatus.Text = "Длительность должна быть положительным целым числом.";
            return;
        }

        decimal? deposit = null;
        if (!string.IsNullOrWhiteSpace(SessionDepositBox.Text))
        {
            if (!decimal.TryParse(SessionDepositBox.Text, NumberStyles.Number,
                    CultureInfo.CurrentCulture, out var parsedDeposit) || parsedDeposit < 0)
            {
                SessionStatus.Text = "Введите корректную сумму депозита (не меньше нуля).";
                return;
            }

            deposit = parsedDeposit;
        }

        var startedAt = date.Date.Add(time);

        try
        {
            using var db = new AppDbContext();
            if (_editingSessionId.HasValue)
            {
                var session = db.GameSessions.FirstOrDefault(s => s.Id == _editingSessionId.Value);
                if (session == null)
                {
                    SessionStatus.Text = "Сеанс не найден.";
                    return;
                }

                session.PlayerId = player.Id;
                session.GameId = game.Id;
                session.StartedAt = startedAt;
                session.DurationMinutes = duration;
                session.Deposit = deposit;
            }
            else
            {
                db.GameSessions.Add(new GameSessions
                {
                    PlayerId = player.Id,
                    GameId = game.Id,
                    StartedAt = startedAt,
                    DurationMinutes = duration,
                    Deposit = deposit
                });
            }

            db.SaveChanges();
            _editingSessionId = null;
            LoadSessions();
            if (PlayersList.SelectedItem is Players selectedPlayer)
                LoadPlayerSessions(selectedPlayer.Id);

            SessionsList.SelectedItem = null;
            SessionPlayerCombo.SelectedItem = null;
            SessionGameCombo.SelectedItem = null;
            SessionDurationBox.Text = SessionDepositBox.Text = "";
            SessionStatus.Text = "Игровой сеанс сохранён.";
        }
        catch (Exception ex)
        {
            SessionStatus.Text = $"Ошибка сохранения: {ex.Message}";
        }
    }

    private void DeleteSession_Click(object? sender, RoutedEventArgs e)
    {
        if (SessionsList.SelectedItem is not GameSessions selected)
        {
            SessionStatus.Text = "Сначала выберите игровой сеанс.";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var session = db.GameSessions.FirstOrDefault(s => s.Id == selected.Id);
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
                LoadPlayerSessions(selectedPlayer.Id);

            SessionStatus.Text = "Игровой сеанс удалён.";
        }
        catch (Exception ex)
        {
            SessionStatus.Text = $"Ошибка удаления: {ex.Message}";
        }
    }
}
