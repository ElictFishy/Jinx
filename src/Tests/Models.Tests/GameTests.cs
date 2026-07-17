using JinxApp.Models;
using Xunit;

namespace Models.Tests;

public class GameTests
{
    private static List<Player> MakePlayers(int count) =>
        Enumerable.Range(1, count)
                  .Select(i => new Player($"P{i}", false))
                  .ToList();

    // --- Constructeur ---

    [Fact]
    public void Game_Constructor_SetsMaxRounds()
    {
        Game game = new Game(5, MakePlayers(2));

        Assert.Equal(5, game.MaxRounds);
    }

    [Fact]
    public void Game_Constructor_CurrentRoundIsOne()
    {
        Game game = new Game(5, MakePlayers(2));

        Assert.Equal(1, game.CurrentRound);
    }

    [Fact]
    public void Game_Constructor_BoardIsNotNull()
    {
        Game game = new Game(5, MakePlayers(2));

        Assert.NotNull(game.Board);
    }

    [Fact]
    public void Game_Constructor_DiceIsNotNull()
    {
        Game game = new Game(5, MakePlayers(2));

        Assert.NotNull(game.Dice);
    }

    [Fact]
    public void Game_Constructor_PlayersAreSet()
    {
        var players = MakePlayers(3);
        Game game = new Game(5, players);

        Assert.Equal(players, game.Players);
    }

    [Fact]
    public void Game_Constructor_CurrentPlayerIsNull()
    {
        Game game = new Game(5, MakePlayers(2));

        Assert.Null(game.CurrentPlayer);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Game_Constructor_VariousMaxRounds_SetsCorrectly(int maxRounds)
    {
        Game game = new Game(maxRounds, MakePlayers(2));

        Assert.Equal(maxRounds, game.MaxRounds);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Game_Constructor_VariousPlayerCounts_SetsCorrectly(int count)
    {
        var players = MakePlayers(count);
        Game game = new Game(5, players);

        Assert.Equal(count, game.Players.Count);
    }

    // --- NextRound ---

    [Fact]
    public void Game_NextRound_IncrementsByOne()
    {
        Game game = new Game(5, MakePlayers(2));

        game.NextRound();

        Assert.Equal(2, game.CurrentRound);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Game_NextRound_CalledNTimes_CurrentRoundIsNPlusOne(int times)
    {
        Game game = new Game(10, MakePlayers(2));

        for (int i = 0; i < times; i++)
            game.NextRound();

        Assert.Equal(1 + times, game.CurrentRound);
    }

    // --- CreateDeck ---

    [Fact]
    public void Game_CreateDeck_Returns48Cards()
    {
        Game game = new Game(5, MakePlayers(2));

        var deck = game.CreateDeck();

        Assert.Equal(48, deck.Count);
    }

    [Fact]
    public void Game_CreateDeck_AllIdsAreUnique()
    {
        Game game = new Game(5, MakePlayers(2));

        var deck = game.CreateDeck();
        var ids = deck.Select(c => c.GetId()).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Game_CreateDeck_ContainsAllColors()
    {
        Game game = new Game(5, MakePlayers(2));

        var deck = game.CreateDeck();
        var colors = deck.Select(c => c.Color).Distinct().ToList();

        Assert.Equal(Enum.GetValues<CardColor>().Length, colors.Count);
    }

    [Fact]
    public void Game_CreateDeck_EachColorHas6Cards()
    {
        Game game = new Game(5, MakePlayers(2));

        var deck = game.CreateDeck();

        foreach (CardColor color in Enum.GetValues<CardColor>())
            Assert.Equal(6, deck.Count(c => c.Color == color));
    }

    [Theory]
    [InlineData(CardColor.RED)]
    [InlineData(CardColor.BLUE)]
    [InlineData(CardColor.GREEN)]
    public void Game_CreateDeck_Color_HasValues1To6(CardColor color)
    {
        Game game = new Game(5, MakePlayers(2));

        var deck = game.CreateDeck();
        var values = deck.Where(c => c.Color == color)
                         .Select(c => c.Value)
                         .OrderBy(v => v)
                         .ToList();

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, values);
    }

    // --- CreateChanceDeck ---

    [Fact]
    public void Game_CreateChanceDeck_ReturnsFullDeck()
    {
        var chanceDeck = Game.CreateChanceDeck();

        // 4 cartes de chacun des 6 types = 24 cartes
        Assert.Equal(24, chanceDeck.Count);
        foreach (ChanceCardType type in Enum.GetValues<ChanceCardType>())
            Assert.Equal(4, chanceDeck.Count(c => c.Type == type));

        // TAKE_LOW / TAKE_HIGH sont à usage unique, les autres réutilisables
        Assert.All(chanceDeck, c =>
        {
            bool expectedSingleUse = c.Type is ChanceCardType.TAKE_LOW_CARD or ChanceCardType.TAKE_HIGH_CARD;
            Assert.Equal(expectedSingleUse, c.IsSingleUse);
        });
    }

    // --- HasBestScore ---

    [Fact]
    public void Game_HasBestScore_ReturnsPlayerWithHighestScore()
    {
        Game game = new Game(5, MakePlayers(2));
        var p1 = new Player("A", false) { Score = 10 };
        var p2 = new Player("B", false) { Score = 20 };

        var best = game.HasBestScore(new List<Player> { p1, p2 });

        Assert.Equal(p2, best);
    }

    [Fact]
    public void Game_HasBestScore_EmptyList_ReturnsNull()
    {
        Game game = new Game(5, MakePlayers(2));

        var best = game.HasBestScore(new List<Player>());

        Assert.Null(best);
    }

    [Fact]
    public void Game_HasBestScore_SinglePlayer_ReturnsThatPlayer()
    {
        Game game = new Game(5, MakePlayers(2));
        var p = new Player("Solo", false) { Score = 5 };

        var best = game.HasBestScore(new List<Player> { p });

        Assert.Equal(p, best);
    }

    public static TheoryData<int[]> ScoreData => new()
    {
        new[] { 5, 10, 3  },   // max = index 1
        new[] { 0, 0,  0  },   // tie → first wins
        new[] { 99, 1, 50 },   // max = index 0
    };

    [Theory]
    [MemberData(nameof(ScoreData))]
    public void Game_HasBestScore_ReturnsMaxScorePlayer(int[] scores)
    {
        Game game = new Game(5, MakePlayers(2));
        var players = scores.Select((s, i) =>
        {
            var p = new Player($"P{i}", false);
            p.Score = s;
            return p;
        }).ToList();

        var best = game.HasBestScore(players);

        Assert.Equal(scores.Max(), best!.Score);
    }

    // --- ToString ---

    [Fact]
    public void Game_ToString_ContainsCurrentRound()
    {
        Game game = new Game(5, MakePlayers(2));

        Assert.Contains("1", game.ToString());
    }

    [Fact]
    public void Game_ToString_ContainsMaxRounds()
    {
        Game game = new Game(7, MakePlayers(2));

        Assert.Contains("7", game.ToString());
    }

    [Fact]
    public void Game_ToString_ContainsPlayerNames()
    {
        var players = new List<Player> { new Player("Alice", false), new Player("Bob", false) };
        Game game = new Game(5, players);

        string result = game.ToString();

        Assert.Contains("Alice", result);
        Assert.Contains("Bob", result);
    }
}