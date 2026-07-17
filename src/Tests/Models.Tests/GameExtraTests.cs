using JinxApp.Models;
using Xunit;

namespace Models.Tests;

public class GameExtraTests
{
    private static List<Player> MakePlayers(int n)
    {
        var list = new List<Player>();
        for (int i = 0; i < n; i++) list.Add(new Player($"P{i}", false));
        return list;
    }

    [Fact]
    public void DistributeChanceCards_GivesTwoCardsPerPlayer()
    {
        var players = MakePlayers(3);
        var game = new Game(3, players);

        game.DistributeChanceCards();

        Assert.All(players, p => Assert.Equal(2, p.ChanceCards.Count));
    }

    [Fact]
    public void DistributeChanceCards_EachPlayerHasOneReusableAndOneSingleUse()
    {
        var players = MakePlayers(1);
        var game = new Game(3, players);

        game.DistributeChanceCards();

        Assert.Contains(players[0].ChanceCards, c => c.IsSingleUse);
        Assert.Contains(players[0].ChanceCards, c => !c.IsSingleUse);
    }

    [Fact]
    public void ToString_ContainsRoundAndPlayerNames()
    {
        var players = new List<Player> { new Player("Alice", false), new Player("Bob", false) };
        var game = new Game(3, players);

        string s = game.ToString();

        Assert.Contains("Tour", s);
        Assert.Contains("Alice", s);
        Assert.Contains("Bob", s);
    }

    [Fact]
    public void NextRound_IncrementsCurrentRound()
    {
        var game = new Game(3, MakePlayers(2));
        int before = game.CurrentRound;

        game.NextRound();

        Assert.Equal(before + 1, game.CurrentRound);
    }

    [Fact]
    public void RefreshCurrentPlayer_DoesNotThrow()
    {
        var game = new Game(3, MakePlayers(1));
        game.RefreshCurrentPlayer();
        Assert.Equal(1, game.CurrentRound);
    }

    [Fact]
    public void HasBestScore_ReturnsHighestScoringPlayer()
    {
        var game = new Game(3, MakePlayers(2));
        var p1 = new Player("A", false) { Score = 5 };
        var p2 = new Player("B", false) { Score = 12 };

        var best = game.HasBestScore(new List<Player> { p1, p2 });

        Assert.Equal(p2, best);
    }

    [Fact]
    public void CurrentPlayer_CanBeSet()
    {
        var players = MakePlayers(2);
        var game = new Game(3, players);

        game.CurrentPlayer = players[1];

        Assert.Equal(players[1], game.CurrentPlayer);
    }
}
