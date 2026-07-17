using JinxApp.Models;

namespace JinxApp.Views;

public partial class ScoreBoardPage : ContentPage
{
	public ScoreBoardPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		LoadScores();
	}

	private void LoadScores()
	{
		List<ScoreEntry> scores = (JinxData.dataService?.LoadHighScores() ?? new())
			.OrderByDescending(s => s.Score)
			.Take(5)
			.ToList();

		bool hasScores = scores.Count > 0;
		TableSection.IsVisible = hasScores;
		EmptySection.IsVisible = !hasScores;

		List<ScoreRowView> rows = scores
			.Select((s, i) => new ScoreRowView
			{
				Rank = (i + 1).ToString(),
				Name = s.IsAi ? $"{s.Name} (IA)" : s.Name,
				ScoreText = s.Score.ToString()
			})
			.ToList();

		BindableLayout.SetItemsSource(ScoreRows, rows);
	}

	private async void OnBtnNewGameClicked(object? sender, EventArgs e)
	{
		await NewGameBtn.ScaleToAsync(0.8, 100);
		await NewGameBtn.ScaleToAsync(1.0, 100);

		await Shell.Current.GoToAsync(nameof(PlayPage));
	}

	private async void OnBtnHistoryClicked(object? sender, EventArgs e)
	{
		await PlayBtn.ScaleToAsync(0.8, 100);
		await PlayBtn.ScaleToAsync(1.0, 100);

		await Shell.Current.GoToAsync($"///{nameof(HistoryPage)}");
	}

	private async void OnBtnRetourClicked(object? sender, EventArgs e)
	{
		await BackBtn.ScaleToAsync(0.8, 100);
		await BackBtn.ScaleToAsync(1.0, 100);

		await Shell.Current.GoToAsync($"///{nameof(HomePage)}");
	}
}

/// <summary>Représentation d'affichage d'une ligne du classement des scores.</summary>
public class ScoreRowView
{
	public string Rank { get; set; } = "";
	public string Name { get; set; } = "";
	public string ScoreText { get; set; } = "";
}
