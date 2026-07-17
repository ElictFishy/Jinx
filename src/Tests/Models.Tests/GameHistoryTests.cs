using JinxApp.Models;
using Xunit;

namespace Models.Tests;

public class GameHistoryTests
{
    private static List<Player> MakePlayers(int count) =>
        Enumerable.Range(1, count)
                  .Select(i => new Player($"P{i}", false))
                  .ToList();

    [Fact]
    public void GameHistory_Constructor_SetsDate()
    {
        DateTime date = new DateTime(2024, 6, 1);
        GameHistory history = new GameHistory(date, MakePlayers(2));

        Assert.Equal(date, history.GetDate());
    }

    [Fact]
    public void GameHistory_Constructor_SetsPlayers()
    {
        var players = MakePlayers(3);
        GameHistory history = new GameHistory(DateTime.Now, players);

        Assert.Equal(players, history.GetPlayers());
    }

    [Fact]
    public void GameHistory_GetPlayers_ReturnsCorrectCount()
    {
        var players = MakePlayers(4);
        GameHistory history = new GameHistory(DateTime.Now, players);

        Assert.Equal(4, history.GetPlayers().Count);
    }

    [Fact]
    public void GameHistory_GetPlayers_EmptyList_ReturnsEmpty()
    {
        GameHistory history = new GameHistory(DateTime.Now, new List<Player>());

        Assert.Empty(history.GetPlayers());
    }

    public static TheoryData<DateTime> DateData => new()
    {
        new DateTime(2000, 1, 1),
        new DateTime(2024, 12, 31),
        DateTime.MinValue,
        DateTime.MaxValue,
    };

    [Theory]
    [MemberData(nameof(DateData))]
    public void GameHistory_GetDate_ReturnsCorrectDate(DateTime date)
    {
        GameHistory history = new GameHistory(date, MakePlayers(2));

        Assert.Equal(date, history.GetDate());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void GameHistory_GetPlayers_VariousPlayerCounts(int count)
    {
        var players = MakePlayers(count);
        GameHistory history = new GameHistory(DateTime.Now, players);

        Assert.Equal(count, history.GetPlayers().Count);
    }
}