using JinxApp.Models;
using JinxApp.Managers;
using Xunit;

namespace Managers.Tests;

public class TurnManagerTests
{
    private static List<Player> CreatePlayers(int count)
    {
        List<Player> players = new();
        for (int i = 0; i < count; i++)
            players.Add(new Player($"Joueur{i + 1}", false));
        return players;
    }


    [Fact]
    public void CurrentPlayer_WithTwoPlayers_ReturnsLastPlayer()
    {
        List<Player> players = CreatePlayers(2);
        TurnManager tm = new TurnManager(players);

        Assert.Equal(players[1], tm.CurrentPlayer);
    }

    [Fact]
    public void CurrentPlayer_WithThreePlayers_ReturnsLastPlayer()
    {
        List<Player> players = CreatePlayers(3);
        TurnManager tm = new TurnManager(players);

        Assert.Equal(players[2], tm.CurrentPlayer);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void CurrentPlayer_AlwaysReturnsLastInList(int count)
    {
        List<Player> players = CreatePlayers(count);
        TurnManager tm = new TurnManager(players);

        Assert.Equal(players[count - 1], tm.CurrentPlayer);
    }


    [Fact]
    public void NextTurn_FromFirstPlayer_ReturnsSecondPlayer()
    {
        List<Player> players = CreatePlayers(3);
        TurnManager tm = new TurnManager(players);

        Player next = tm.NextTurn(players, players[0]);

        Assert.Equal(players[1], next);
    }

    [Fact]
    public void NextTurn_FromSecondPlayer_ReturnsThirdPlayer()
    {
        List<Player> players = CreatePlayers(3);
        TurnManager tm = new TurnManager(players);

        Player next = tm.NextTurn(players, players[1]);

        Assert.Equal(players[2], next);
    }

    [Fact]
    public void NextTurn_FromLastPlayer_WrapsToFirstPlayer()
    {
        List<Player> players = CreatePlayers(3);
        TurnManager tm = new TurnManager(players);

        Player next = tm.NextTurn(players, players[2]);

        Assert.Equal(players[0], next);
    }

    [Fact]
    public void NextTurn_WithTwoPlayers_AlternatesCorrectly()
    {
        List<Player> players = CreatePlayers(2);
        TurnManager tm = new TurnManager(players);

        Player second = tm.NextTurn(players, players[0]);
        Player first = tm.NextTurn(players, second);

        Assert.Equal(players[1], second);
        Assert.Equal(players[0], first);
    }

    /// <summary>
    /// Vérifie que NextTurn parcourt tous les joueurs dans l'ordre
    /// et revient au point de départ après un tour complet.
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void NextTurn_FullRotation_ReturnsToStartingPlayer(int count)
    {
        List<Player> players = CreatePlayers(count);
        TurnManager tm = new TurnManager(players);

        Player current = players[0];
        for (int i = 0; i < count; i++)
            current = tm.NextTurn(players, current);

        Assert.Equal(players[0], current);
    }

    /// <summary>Vérifie l'ordre exact des joueurs sur un tour complet à 4 joueurs.</summary>
    [Fact]
    public void NextTurn_FourPlayers_CorrectOrder()
    {
        List<Player> players = CreatePlayers(4);
        TurnManager tm = new TurnManager(players);

        Player p1 = tm.NextTurn(players, players[0]);
        Player p2 = tm.NextTurn(players, p1);
        Player p3 = tm.NextTurn(players, p2);
        Player p4 = tm.NextTurn(players, p3);

        Assert.Equal(players[1], p1);
        Assert.Equal(players[2], p2);
        Assert.Equal(players[3], p3);
        Assert.Equal(players[0], p4); // wrap
    }

    /// <summary>
    /// Paramétrise : depuis l'index donné, le suivant attendu (liste de 4 joueurs).
    /// </summary>
    public static TheoryData<int, int> NextTurnIndexData => new()
    {
        { 0, 1 },
        { 1, 2 },
        { 2, 3 },
        { 3, 0 }, // wrap
    };

    [Theory]
    [MemberData(nameof(NextTurnIndexData))]
    public void NextTurn_FourPlayers_CorrectNextIndex(int currentIndex, int expectedIndex)
    {
        List<Player> players = CreatePlayers(4);
        TurnManager tm = new TurnManager(players);

        Player next = tm.NextTurn(players, players[currentIndex]);

        Assert.Equal(players[expectedIndex], next);
    }
}