using JinxApp.Models;
using JinxApp.Managers;
using Xunit;

namespace Managers.Tests;

public class GameManagerTests
{
    private static List<Player> CreatePlayers(int count = 2)
    {
        List<Player> players = new();
        for (int i = 0; i < count; i++)
            players.Add(new Player($"Joueur{i + 1}", false));
        return players;
    }

    private static GameManager CreateGameManager(List<Player>? players = null)
    {
        players ??= CreatePlayers();
        Game game = new Game(3, players);
        game.Board.Setup(game.CreateDeck());
        game.CurrentPlayer = players[0];

        return new GameManager(
            game,
            new TurnManager(players),
            new VictoryManager(),
            new DiceManager(),
            new ChanceCardManager(),
            new PlacementManager(),
            new AiManager()
        );
    }


    [Fact]
    public void RollDice_ReturnsValueBetween1And6()
    {
        GameManager gm = CreateGameManager();
        int result = gm.RollDice();
        Assert.InRange(result, 1, 6);
    }

    [Fact]
    public void RollDice_UpdatesDiceValue()
    {
        GameManager gm = CreateGameManager();
        gm.RollDice();
        Assert.InRange(gm.Game.Dice.Value, 1, 6);
    }

    [Fact]
    public void ReRollDice_AfterRoll_ReturnsValueBetween1And6()
    {
        GameManager gm = CreateGameManager();
        gm.RollDice();
        int result = gm.ReRollDice();
        Assert.InRange(result, 1, 6);
    }

    [Fact]
    public void ReRollDice_WithoutRolling_ThrowsException()
    {
        GameManager gm = CreateGameManager();
        gm.RollDice();
        gm.ReRollDice();
        Assert.Throws<InvalidOperationException>(() => gm.ReRollDice());
    }

    [Fact]
    public void RollDice_ResetsRerollFlag()
    {
        GameManager gm = CreateGameManager();
        gm.RollDice();
        gm.ReRollDice();
        gm.RollDice();
        int result = gm.ReRollDice();
        Assert.InRange(result, 1, 6);
    }


    [Fact]
    public void CanPickCard_WithMatchingDiceValue_ReturnsTrue()
    {
        GameManager gm = CreateGameManager();
        NumberCard?[,] grid = gm.Game.Board.Grid;
        NumberCard? card = null;
        int row = 0, col = 0;

        for (int r = 0; r < Board.SIZE && card == null; r++)
            for (int c = 0; c < Board.SIZE && card == null; c++)
                if (grid[r, c] != null) { card = grid[r, c]; row = r; col = c; }

        Assert.NotNull(card);
        gm.Game.Dice.Value = card!.Value;

        Assert.True(gm.CanPickCard(row, col, gm.Game.Board, gm.Game.Dice.Value));
    }

    [Fact]
    public void CanPickCard_WithWrongDiceValue_ReturnsFalse()
    {
        GameManager gm = CreateGameManager();
        NumberCard?[,] grid = gm.Game.Board.Grid;
        NumberCard? card = grid[0, 0];
        Assert.NotNull(card);
        int wrongValue = card!.Value == 6 ? 1 : card.Value + 1;
        Assert.False(gm.CanPickCard(0, 0, gm.Game.Board, wrongValue));
    }

    [Fact]
    public void PickCard_AddsCardToCurrentPlayer()
    {
        GameManager gm = CreateGameManager();
        NumberCard?[,] grid = gm.Game.Board.Grid;
        NumberCard? card = null;
        int row = 0, col = 0;

        for (int r = 0; r < Board.SIZE && card == null; r++)
            for (int c = 0; c < Board.SIZE && card == null; c++)
                if (grid[r, c] != null) { card = grid[r, c]; row = r; col = c; }

        Assert.NotNull(card);
        gm.Game.Dice.Value = card!.Value;
        gm.PickCard(row, col);

        // La carte dans la main est un clone (même valeur et couleur, IsActive=true)
        // On vérifie par valeur et couleur plutôt que par référence objet
        Assert.Contains(gm.CurrentPlayer!.NumberCards,
            c => c.Value == card!.Value && c.Color == card.Color && c.IsActive);
    }

    [Fact]
    public void PickCard_RemovesCardFromBoard()
    {
        GameManager gm = CreateGameManager();
        NumberCard?[,] grid = gm.Game.Board.Grid;
        NumberCard? card = grid[0, 0];

        Assert.NotNull(card);
        gm.Game.Dice.Value = card!.Value;
        gm.PickCard(0, 0);

        // La carte reste dans la grille mais est marquée inactive (IsActive=false)
        // Elle est donc logiquement "retirée" du plateau
        Assert.True(gm.Game.Board.Grid[0, 0] == null || !gm.Game.Board.Grid[0, 0]!.IsActive);
    }


    [Fact]
    public void EndTurn_ChangesCurrentPlayer()
    {
        List<Player> players = CreatePlayers(2);
        GameManager gm = CreateGameManager(players);
        Player? firstPlayer = gm.CurrentPlayer;

        gm.EndTurn();

        Assert.NotEqual(firstPlayer, gm.CurrentPlayer);
    }

    [Fact]
    public void EndTurn_CyclesBackToFirstPlayer()
    {
        List<Player> players = CreatePlayers(2);
        GameManager gm = CreateGameManager(players);
        Player? firstPlayer = gm.CurrentPlayer;

        gm.EndTurn();
        gm.EndTurn();

        Assert.Equal(firstPlayer, gm.CurrentPlayer);
    }


    [Fact]
    public void IsGameOver_AtStart_ReturnsFalse()
    {
        GameManager gm = CreateGameManager();
        Assert.False(gm.IsGameOver());
    }

    [Fact]
    public void IsGameOver_AfterMaxRounds_ReturnsTrue()
    {
        GameManager gm = CreateGameManager();
        gm.EndRound();
        gm.EndRound();
        gm.EndRound();

        Assert.True(gm.IsGameOver());
    }

    [Fact]
    public void IsRoundOver_WhenDiceMatchesCard_ReturnsFalse()
    {
        GameManager gm = CreateGameManager();
        NumberCard? card = gm.Game.Board.Grid[0, 0];
        Assert.NotNull(card);
        gm.Game.Dice.Value = card!.Value;

        Assert.False(gm.IsRoundOver());
    }

    [Fact]
    public void IsRoundOver_WhenNoCardMatchesDice_ReturnsTrue()
    {
        GameManager gm = CreateGameManager();
        for (int r = 0; r < Board.SIZE; r++)
            for (int c = 0; c < Board.SIZE; c++)
                gm.Game.Board.RemoveCard(r, c);

        Assert.True(gm.IsRoundOver());
    }


    [Fact]
    public void EndRound_IncrementsCurrentRound()
    {
        GameManager gm = CreateGameManager();
        int roundBefore = gm.Game.CurrentRound;
        gm.EndRound();
        Assert.Equal(roundBefore + 1, gm.Game.CurrentRound);
    }

    [Fact]
    public void EndRound_ResetsBoard()
    {
        GameManager gm = CreateGameManager();
        gm.Game.Board.RemoveCard(0, 0);
        gm.Game.Board.RemoveCard(1, 1);

        gm.EndRound();

        // Après EndRound, toutes les cartes du nouveau plateau doivent être actives
        int count = 0;
        foreach (NumberCard? card in gm.Game.Board.Grid)
            if (card != null && card.IsActive) count++;

        Assert.Equal(16, count);
    }

    [Fact]
    public void EndRound_ComputesScores()
    {
        List<Player> players = CreatePlayers(2);
        GameManager gm = CreateGameManager(players);
        players[0].AddNumberCard(new NumberCard(1, 5, CardColor.RED));
        gm.EndRound();
        Assert.True(players[0].Score >= 0);
    }


    [Fact]
    public void GetColorsOnBoard_ReturnsNonEmptyCollection()
    {
        GameManager gm = CreateGameManager();
        IEnumerable<CardColor> colors = gm.GetColorsOnBoard();
        Assert.NotEmpty(colors);
    }

    [Fact]
    public void GetColorsOnBoard_EmptyBoard_ReturnsEmptyCollection()
    {
        GameManager gm = CreateGameManager();
        for (int r = 0; r < Board.SIZE; r++)
            for (int c = 0; c < Board.SIZE; c++)
                gm.Game.Board.RemoveCard(r, c);

        IEnumerable<CardColor> colors = gm.GetColorsOnBoard();
        Assert.Empty(colors);
    }
}