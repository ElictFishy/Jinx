using JinxApp.Models;
using JinxApp.Managers;
using Xunit;

namespace Managers.Tests;

public class VictoryManagerTests
{

    private static Player CreatePlayer(string name = "Player") => new Player(name, false);

    private static Board CreateEmptyBoard() => new Board();

    private static Board CreateBoardWithCard(int row, int col, int value, CardColor color)
    {
        Board board = new Board();
        board.Grid[row, col] = new NumberCard(1, value, color);
        return board;
    }

    private static Game CreateGame(int maxRounds = 3, int playerCount = 2)
    {
        List<Player> players = new();
        for (int i = 0; i < playerCount; i++)
            players.Add(new Player($"Joueur{i + 1}", false));
        Game game = new Game(maxRounds, players);
        game.Board.Setup(game.CreateDeck());
        game.CurrentPlayer = players[0];
        return game;
    }

    [Fact]
    public void CanPlay_CardMatchesDice_ReturnsTrue()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateBoardWithCard(0, 0, 4, CardColor.RED);
        Dice dice = new Dice(6);
        dice.Value = 4;

        Assert.True(vm.CanPlay(board, dice));
    }

    [Fact]
    public void CanPlay_NoCardMatchesDice_ReturnsFalse()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED);
        Dice dice = new Dice(6);
        dice.Value = 5;

        Assert.False(vm.CanPlay(board, dice));
    }

    [Fact]
    public void CanPlay_EmptyBoard_ReturnsFalse()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateEmptyBoard();
        Dice dice = new Dice(6);
        dice.Value = 3;

        Assert.False(vm.CanPlay(board, dice));
    }

    [Fact]
    public void CanPlay_MatchOnlyInOneCell_ReturnsTrue()
    {
        VictoryManager vm = new VictoryManager();
        Board board = new Board();
        board.Grid[0, 0] = new NumberCard(1, 2, CardColor.RED);
        board.Grid[1, 1] = new NumberCard(2, 5, CardColor.BLUE);
        Dice dice = new Dice(6);
        dice.Value = 5;

        Assert.True(vm.CanPlay(board, dice));
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(1, 2, false)]
    [InlineData(6, 6, true)]
    [InlineData(6, 3, false)]
    public void CanPlay_VariousDiceValues_ReturnsExpected(int cardValue, int diceValue, bool expected)
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateBoardWithCard(0, 0, cardValue, CardColor.GREEN);
        Dice dice = new Dice(6);
        dice.Value = diceValue;

        Assert.Equal(expected, vm.CanPlay(board, dice));
    }

    [Fact]
    public void IsGameOver_RoundBelowMax_ReturnsFalse()
    {
        VictoryManager vm = new VictoryManager();
        Game game = CreateGame(maxRounds: 3);

        Assert.False(vm.IsGameOver(game));
    }

    [Fact]
    public void IsGameOver_AfterAllRounds_ReturnsTrue()
    {
        VictoryManager vm = new VictoryManager();
        Game game = CreateGame(maxRounds: 3);
        game.NextRound();
        game.NextRound();
        game.NextRound();
        game.NextRound(); // CurrentRound = 4 > MaxRounds = 3

        Assert.True(vm.IsGameOver(game));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    public void IsGameOver_VariousRounds_ReturnsExpected(int extraNextRounds, bool expected)
    {
        VictoryManager vm = new VictoryManager();
        Game game = CreateGame(maxRounds: 3);
        for (int i = 0; i < extraNextRounds; i++)
        {
            game.NextRound();
        }

        Assert.Equal(expected, vm.IsGameOver(game));
    }

    [Fact]
    public void GetWinner_ReturnsPlayerWithHighestScore()
    {
        VictoryManager vm = new VictoryManager();
        Player p1 = CreatePlayer("Alice");
        Player p2 = CreatePlayer("Bob");
        p1.Score = 10;
        p2.Score = 20;

        Player winner = vm.GetWinner(new List<Player> { p1, p2 });

        Assert.Equal(p2, winner);
    }

    [Fact]
    public void GetWinner_SinglePlayer_ReturnsThatPlayer()
    {
        VictoryManager vm = new VictoryManager();
        Player p1 = CreatePlayer("Solo");
        p1.Score = 5;

        Player winner = vm.GetWinner(new List<Player> { p1 });

        Assert.Equal(p1, winner);
    }

    [Fact]
    public void GetWinner_AllSameScore_ReturnsOnePlayer()
    {
        VictoryManager vm = new VictoryManager();
        Player p1 = CreatePlayer("A");
        Player p2 = CreatePlayer("B");
        p1.Score = 10;
        p2.Score = 10;

        Player winner = vm.GetWinner(new List<Player> { p1, p2 });

        Assert.NotNull(winner);
    }

    public static TheoryData<int[], int> GetWinnerData => new()
    {
        { new[] { 5, 10, 3 }, 1 },
        { new[] { 20, 1, 15 }, 0 },
        { new[] { 0, 0, 7 }, 2 },
    };

    [Theory]
    [MemberData(nameof(GetWinnerData))]
    public void GetWinner_ThreePlayers_ReturnsHighestScorer(int[] scores, int expectedIndex)
    {
        VictoryManager vm = new VictoryManager();
        List<Player> players = new();
        for (int i = 0; i < scores.Length; i++)
        {
            Player p = CreatePlayer($"P{i}");
            p.Score = scores[i];
            players.Add(p);
        }

        Player winner = vm.GetWinner(players);

        Assert.Equal(players[expectedIndex], winner);
    }

    [Fact]
    public void GetColorsOnBoard_EmptyBoard_ReturnsEmpty()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateEmptyBoard();

        Assert.Empty(vm.GetColorsOnBoard(board));
    }

    [Fact]
    public void GetColorsOnBoard_OneCard_ReturnsItsColor()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED);

        IEnumerable<CardColor> colors = vm.GetColorsOnBoard(board);

        Assert.Contains(CardColor.RED, colors);
        Assert.Single(colors);
    }

    [Fact]
    public void GetColorsOnBoard_TwoCardsOfSameColor_ReturnsOnce()
    {
        VictoryManager vm = new VictoryManager();
        Board board = new Board();
        board.Grid[0, 0] = new NumberCard(1, 2, CardColor.BLUE);
        board.Grid[1, 1] = new NumberCard(2, 4, CardColor.BLUE);

        IEnumerable<CardColor> colors = vm.GetColorsOnBoard(board);

        Assert.Single(colors);
        Assert.Contains(CardColor.BLUE, colors);
    }

    [Fact]
    public void GetColorsOnBoard_TwoDifferentColors_ReturnsBoth()
    {
        VictoryManager vm = new VictoryManager();
        Board board = new Board();
        board.Grid[0, 0] = new NumberCard(1, 2, CardColor.RED);
        board.Grid[1, 1] = new NumberCard(2, 4, CardColor.GREEN);

        IEnumerable<CardColor> colors = vm.GetColorsOnBoard(board).ToList();

        Assert.Equal(2, colors.Count());
        Assert.Contains(CardColor.RED, colors);
        Assert.Contains(CardColor.GREEN, colors);
    }

    [Fact]
    public void RemoveMatchingColorCards_NoMatch_PlayerHandUnchanged()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED);
        Player player = CreatePlayer();
        player.AddNumberCard(new NumberCard(1, 4, CardColor.BLUE));

        vm.RemoveMatchingColorCards(player, board);

        Assert.Single(player.NumberCards);
    }

    [Fact]
    public void RemoveMatchingColorCards_MatchingColor_RemovesCard()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED);
        Player player = CreatePlayer();
        player.AddNumberCard(new NumberCard(1, 4, CardColor.RED));

        vm.RemoveMatchingColorCards(player, board);

        Assert.Empty(player.NumberCards);
    }

    [Fact]
    public void RemoveMatchingColorCards_MixedHand_RemovesOnlyMatching()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED);
        Player player = CreatePlayer();
        player.AddNumberCard(new NumberCard(1, 4, CardColor.RED));
        player.AddNumberCard(new NumberCard(2, 5, CardColor.BLUE));

        vm.RemoveMatchingColorCards(player, board);

        Assert.Single(player.NumberCards);
        Assert.Equal(CardColor.BLUE, player.NumberCards[0].Color);
    }

    [Fact]
    public void RemoveMatchingColorCards_EmptyBoard_RemovesNothing()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateEmptyBoard();
        Player player = CreatePlayer();
        player.AddNumberCard(new NumberCard(1, 4, CardColor.RED));
        player.AddNumberCard(new NumberCard(2, 5, CardColor.BLUE));

        vm.RemoveMatchingColorCards(player, board);

        Assert.Equal(2, player.NumberCards.Count);
    }

    [Fact]
    public void RemoveMatchingColorCards_EmptyHand_DoesNotThrow()
    {
        VictoryManager vm = new VictoryManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED);
        Player player = CreatePlayer();

        var ex = Record.Exception(() => vm.RemoveMatchingColorCards(player, board));
        Assert.Null(ex);
    }

    [Fact]
    public void ComputeScores_EmptyHand_ScoreUnchanged()
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();

        vm.ComputeScores(new[] { player });

        Assert.Equal(0, player.Score);
    }

    [Fact]
    public void ComputeScores_WithCards_AddsCorrectSum()
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        player.AddNumberCard(new NumberCard(1, 3, CardColor.RED));
        player.AddNumberCard(new NumberCard(2, 5, CardColor.BLUE));

        vm.ComputeScores(new[] { player });

        Assert.Equal(8, player.Score);
    }

    [Fact]
    public void ComputeScores_CalledTwice_IsIdempotent()
    {
        // Le score reflète la somme des cartes détenues (pas un cumul) :
        // recalculer plusieurs fois donne toujours le même résultat.
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        player.AddNumberCard(new NumberCard(1, 4, CardColor.GREEN));

        vm.ComputeScores(new[] { player });
        vm.ComputeScores(new[] { player });

        Assert.Equal(4, player.Score);
    }

    [Fact]
    public void ComputeScores_MultiplePlayers_EachScoreIndependent()
    {
        VictoryManager vm = new VictoryManager();
        Player p1 = CreatePlayer("A");
        Player p2 = CreatePlayer("B");
        p1.AddNumberCard(new NumberCard(1, 3, CardColor.RED));
        p2.AddNumberCard(new NumberCard(2, 6, CardColor.BLUE));

        vm.ComputeScores(new[] { p1, p2 });

        Assert.Equal(3, p1.Score);
        Assert.Equal(6, p2.Score);
    }

    public static TheoryData<int[], int> ComputeScoresData => new()
    {
        { new[] { 1, 2, 3 }, 6 },
        { new[] { 6, 6 }, 12 },
        { new[] { 1 }, 1 },
        { Array.Empty<int>(), 0 },
    };

    [Theory]
    [MemberData(nameof(ComputeScoresData))]
    public void ComputeScores_VariousCards_ReturnsExpectedTotal(int[] values, int expectedScore)
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        int id = 1;
        foreach (int v in values)
            player.AddNumberCard(new NumberCard(id++, v, CardColor.RED));

        vm.ComputeScores(new[] { player });

        Assert.Equal(expectedScore, player.Score);
    }

    [Fact]
    public void GetStrongestCards_EmptyHand_ReturnsEmptyList()
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();

        List<NumberCard> result = vm.GetStrongestCards(player);

        Assert.Empty(result);
    }

    [Fact]
    public void GetStrongestCards_SingleCard_ReturnsThatCard()
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        NumberCard card = new NumberCard(1, 5, CardColor.RED);
        player.AddNumberCard(card);

        List<NumberCard> result = vm.GetStrongestCards(player);

        Assert.Single(result);
        Assert.Equal(5, result[0].Value);
    }

    [Fact]
    public void GetStrongestCards_MultipleCards_ReturnsMaxOnly()
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        player.AddNumberCard(new NumberCard(1, 3, CardColor.RED));
        player.AddNumberCard(new NumberCard(2, 6, CardColor.BLUE));
        player.AddNumberCard(new NumberCard(3, 2, CardColor.GREEN));

        List<NumberCard> result = vm.GetStrongestCards(player);

        Assert.Single(result);
        Assert.Equal(6, result[0].Value);
    }

    [Fact]
    public void GetStrongestCards_TieForMax_ReturnsAllTied()
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        player.AddNumberCard(new NumberCard(1, 6, CardColor.RED));
        player.AddNumberCard(new NumberCard(2, 6, CardColor.BLUE));
        player.AddNumberCard(new NumberCard(3, 3, CardColor.GREEN));

        List<NumberCard> result = vm.GetStrongestCards(player);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(6, c.Value));
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, 3)]
    [InlineData(new[] { 6, 6, 6 }, 6)]
    [InlineData(new[] { 1 }, 1)]
    [InlineData(new[] { 4, 2, 5, 1 }, 5)]
    public void GetStrongestCards_VariousHands_MaxValueCorrect(int[] values, int expectedMax)
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        int id = 1;
        foreach (int v in values)
            player.AddNumberCard(new NumberCard(id++, v, CardColor.RED));

        List<NumberCard> result = vm.GetStrongestCards(player);

        Assert.All(result, c => Assert.Equal(expectedMax, c.Value));
    }

    [Fact]
    public void RemoveStrongestCard_ValidCard_ReturnsRemovedCard()
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        NumberCard card = new NumberCard(1, 6, CardColor.RED);
        player.AddNumberCard(card);

        NumberCard? result = vm.RemoveStrongestCard(player, card);

        Assert.Equal(card, result);
    }

    [Fact]
    public void RemoveStrongestCard_ValidCard_RemovesFromHand()
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        NumberCard card = new NumberCard(1, 6, CardColor.RED);
        player.AddNumberCard(card);

        vm.RemoveStrongestCard(player, card);

        Assert.DoesNotContain(card, player.NumberCards);
    }

    [Fact]
    public void RemoveStrongestCard_OneOfTwo_OnlyChosenRemoved()
    {
        VictoryManager vm = new VictoryManager();
        Player player = CreatePlayer();
        NumberCard card1 = new NumberCard(1, 6, CardColor.RED);
        NumberCard card2 = new NumberCard(2, 6, CardColor.BLUE);
        player.AddNumberCard(card1);
        player.AddNumberCard(card2);

        vm.RemoveStrongestCard(player, card1);

        Assert.Single(player.NumberCards);
        Assert.Contains(card2, player.NumberCards);
    }
}

