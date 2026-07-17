using JinxApp.Managers;
using JinxApp.Models;

namespace JinxApp.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        bool hasSave = JinxData.dataService?.HasCurrentGame() ?? false;
        LoadGame.IsEnabled = hasSave;
        LoadGame.Opacity = hasSave ? 1.0 : 0.4;
    }

    private async void OnBtnNewGameClicked(object? sender, EventArgs e)
    {
        await NewGame.ScaleToAsync(0.8, 100);
        await NewGame.ScaleToAsync(1.0, 100);
        await Shell.Current.GoToAsync(nameof(PlayPage));
    }

    private async void OnBtnLoadGameClicked(object? sender, EventArgs e)
    {
        await LoadGame.ScaleToAsync(0.8, 100);
        await LoadGame.ScaleToAsync(1.0, 100);

        Game? savedGame = JinxData.dataService?.LoadCurrentGame();
        if (savedGame == null)
        {
            await DisplayAlertAsync("Erreur", "Aucune partie sauvegardée.", "OK");
            return;
        }

        JinxData.g = savedGame;
        JinxData.IsDemo = false;
        JinxData.gm = new GameManager(
            savedGame,
            new TurnManager(savedGame.Players),
            new VictoryManager(),
            new DiceManager(),
            new ChanceCardManager(),
            new PlacementManager(),
            new AiManager()
        );

        await Shell.Current.GoToAsync(nameof(GamePage));
    }
    private async void OnBtnRulesClicked(object? sender, EventArgs e)
    {
        await Rule.ScaleToAsync(0.8, 100);
        await Rule.ScaleToAsync(1.0, 100);
        await Shell.Current.GoToAsync(nameof(RulesPage));
    }

    private async void OnBtnScoreboardClicked(object? sender, EventArgs e)
    {
        await Scoreboard.ScaleToAsync(0.8, 100);
        await Scoreboard.ScaleToAsync(1.0, 100);
        await Shell.Current.GoToAsync(nameof(ScoreBoardPage));
    }
}