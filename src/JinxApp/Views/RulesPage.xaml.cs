namespace JinxApp.Views;

using JinxApp.Models;
using JinxApp.Managers;

public partial class RulesPage : ContentPage
{
    public RulesPage()
    {
        InitializeComponent();
    }

    private async void OnBtnExempleClicked(object? sender, EventArgs e)
    {
        await ExempleBtn.ScaleToAsync(0.8, 100);
        await ExempleBtn.ScaleToAsync(1.0, 100);

        // Partie de démonstration : deux IA s'affrontent. Rien n'est sauvegardé
        // (ni partie courante, ni historique, ni classement) — voir JinxData.IsDemo.
        List<Player> players = new()
        {
            new Player("IA Bleue", true),
            new Player("IA Rouge", true),
        };

        Game game = new Game(3, players);
        game.Board.Setup(game.DrawBoardCards());

        JinxData.g = game;
        JinxData.gm = new GameManager(
            game,
            new TurnManager(players),
            new VictoryManager(),
            new DiceManager(),
            new ChanceCardManager(),
            new PlacementManager(),
            new AiManager()
        );

        JinxData.g.CurrentPlayer = game.Players.OrderBy(_ => Random.Shared.Next()).First();
        JinxData.IsDemo = true;

        await Shell.Current.GoToAsync(nameof(GamePage));
    }

    private async void OnBtnRetourClicked(object? sender, EventArgs e)
    {
        await BackBtn.ScaleToAsync(0.8, 100);
        await BackBtn.ScaleToAsync(1.0, 100);

        await Shell.Current.GoToAsync($"///{nameof(HomePage)}");
    }
}