using JinxApp.Models;

namespace JinxApp.Views;

public partial class HistoryPage : ContentPage
{
    public HistoryPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadHistory();
    }

    private void LoadHistory()
    {
        List<GameHistory> history = JinxData.dataService?.LoadHistory() ?? new();

        // Du plus récent au plus ancien.
        List<HistoryEntryView> entries = history
            .OrderByDescending(h => h.GetDate())
            .Select(BuildEntry)
            .ToList();

        HistoryList.ItemsSource = entries;
    }

    private static HistoryEntryView BuildEntry(GameHistory game)
    {
        List<Player> players = game.GetPlayers() ?? new();
        int bestScore = players.Count > 0 ? players.Max(p => p.Score) : 0;

        List<HistoryRowView> rows = players
            .OrderByDescending(p => p.Score)
            .Select(p => new HistoryRowView
            {
                PlayerName = p.IsAi ? $"{p.Name} (IA)" : p.Name,
                ScoreText = p.Score.ToString(),
                RowColor = p.Score == bestScore
                    ? Color.FromArgb("#80FFD700")   // doré pour le(s) gagnant(s)
                    : Color.FromArgb("#80CCCCCC")
            })
            .ToList();

        return new HistoryEntryView
        {
            DateText = game.GetDate().ToString("dd / MM / yy : HH'h'mm"),
            Rows = rows
        };
    }

    private async void OnBtnRetourClicked(object? sender, EventArgs e)
    {
        await BackBtn.ScaleToAsync(0.8, 100);
        await BackBtn.ScaleToAsync(1.0, 100);
        await Shell.Current.GoToAsync($"///{nameof(HomePage)}");
    }
}

/// <summary>Représentation d'affichage d'une partie terminée dans l'historique.</summary>
public class HistoryEntryView
{
    public string DateText { get; set; } = "";
    public List<HistoryRowView> Rows { get; set; } = new();
}

/// <summary>Une ligne joueur/score dans le tableau d'une partie de l'historique.</summary>
public class HistoryRowView
{
    public string PlayerName { get; set; } = "";
    public string ScoreText { get; set; } = "";
    public Color RowColor { get; set; } = Colors.Transparent;
}
