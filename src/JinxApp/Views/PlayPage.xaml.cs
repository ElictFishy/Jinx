namespace JinxApp.Views;

using JinxApp.Models;
using JinxApp.Managers;
using System.ComponentModel;

public partial class PlayPage : ContentPage
{
    public bool CanPlay =>
        !string.IsNullOrWhiteSpace(LblJoueur1.Text) &&
        !string.IsNullOrWhiteSpace(LblJoueur2.Text) &&
        (AjouterJoueur3.IsVisible || !string.IsNullOrWhiteSpace(LblJoueur3.Text)) &&
        (AjouterJoueur4.IsVisible || !string.IsNullOrWhiteSpace(LblJoueur4.Text));

    public PlayPage()
    {
        InitializeComponent();
        BindingContext = this;

        LblJoueur1.TextChanged += (s, e) => OnPropertyChanged(nameof(CanPlay));
        LblJoueur2.TextChanged += (s, e) => OnPropertyChanged(nameof(CanPlay));
        LblJoueur3.TextChanged += (s, e) => OnPropertyChanged(nameof(CanPlay));
        LblJoueur4.TextChanged += (s, e) => OnPropertyChanged(nameof(CanPlay));
    }

    private void OnJoueur3Checked(object? sender, CheckedChangedEventArgs e)
    {
        AjouterJoueur3.IsVisible = !e.Value;
        LayoutJoueur3.IsVisible = e.Value;
        BorderJoueur4.IsVisible = e.Value;

        if (!e.Value)
        {
            LayoutJoueur4.IsVisible = false;
            AjouterJoueur4.IsVisible = true;
            BorderJoueur4.IsVisible = false;
            ChkJoueur4.IsChecked = false;
            ChkIA4.IsChecked = false;
        }
    }

    private void OnJoueur4Checked(object? sender, CheckedChangedEventArgs e)
    {
        AjouterJoueur4.IsVisible = !e.Value;
        LayoutJoueur4.IsVisible = e.Value;
    }

    private void OnRetirerJoueur3Clicked(object? sender, EventArgs e)
    {
        AjouterJoueur3.IsVisible = true;
        LayoutJoueur3.IsVisible = false;
        ChkJoueur3.IsChecked = false;
        ChkIA3.IsChecked = false;

        BorderJoueur4.IsVisible = false;
        LayoutJoueur4.IsVisible = false;
        AjouterJoueur4.IsVisible = true;
        ChkJoueur4.IsChecked = false;
        ChkIA4.IsChecked = false;
    }

    private void OnRetirerJoueur4Clicked(object? sender, EventArgs e)
    {
        AjouterJoueur4.IsVisible = true;
        LayoutJoueur4.IsVisible = false;
        ChkJoueur4.IsChecked = false;
        ChkIA4.IsChecked = false;
    }

    private async void OnBtnPlayClicked(object? sender, EventArgs e)
    {
        await PlayBtn.ScaleToAsync(0.8, 100);
        await PlayBtn.ScaleToAsync(1.0, 100);

        // Création des joueurs

        Player p1 = new Player(LblJoueur1.Text, false);
        Player p2 = new Player(LblJoueur2.Text, ChkIA2.IsChecked);

        List<Player> players = new() { p1, p2 };
        if (!AjouterJoueur3.IsVisible)
            players.Add(new Player(LblJoueur3.Text, ChkIA3.IsChecked));
        if (!AjouterJoueur4.IsVisible)
            players.Add(new Player(LblJoueur4.Text, ChkIA4.IsChecked));

        // Création de la partie
        Game game = new Game(3, players);
        game.Board.Setup(game.DrawBoardCards());

        // Aucune carte chance distribuée au départ : joueurs et IA commencent
        // sans carte chance (ils peuvent en piocher en cours de partie).

        JinxData.g = game;
        JinxData.IsDemo = false;

        JinxData.gm = new GameManager(
            game,
            new TurnManager(players),
            new VictoryManager(),
            new DiceManager(),
            new ChanceCardManager(),
            new PlacementManager(),
            new AiManager()
        );

        Player firstPlayer = game.Players.OrderBy(_ => Random.Shared.Next()).First();
        JinxData.g.CurrentPlayer = firstPlayer;

        JinxData.dataService?.SaveCurrentGame(JinxData.g);

        await Shell.Current.GoToAsync(nameof(GamePage));
    }

    private async void OnBtnRetourClicked(object? sender, EventArgs e)
    {
        await BackBtn.ScaleToAsync(0.8, 100);
        await BackBtn.ScaleToAsync(1.0, 100);
        await Shell.Current.GoToAsync($"///{nameof(HomePage)}");
    }
}