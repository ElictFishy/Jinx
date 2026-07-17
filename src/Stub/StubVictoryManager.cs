using JinxApp.Models;
using JinxApp.Managers;

namespace JinxApp.Stubs;

/// <summary>
/// Stub du gestionnaire de victoire.
/// Toutes les valeurs retournées sont contrôlables depuis le test.
/// </summary>
public class StubVictoryManager : IVictoryManager
{
    public bool CanPlayValue { get; set; } = true;
    public bool IsGameOverValue { get; set; } = false;
    public Player? WinnerToReturn { get; set; }
    public IEnumerable<CardColor> ColorsOnBoardToReturn { get; set; } = new List<CardColor>();
    public List<NumberCard> StrongestCardsToReturn { get; set; } = new();

    public int CanPlayCallCount { get; private set; } = 0;
    public int IsGameOverCallCount { get; private set; } = 0;
    public int ComputeScoresCallCount { get; private set; } = 0;
    public int RemoveMatchingColorCardsCallCount { get; private set; } = 0;

    public bool CanPlay(Board board, Dice dice)
    {
        CanPlayCallCount++;
        return CanPlayValue;
    }

    public bool IsGameOver(Game game)
    {
        IsGameOverCallCount++;
        return IsGameOverValue;
    }

    public Player GetWinner(List<Player> players)
    {
        return WinnerToReturn ?? players.First();
    }

    public IEnumerable<CardColor> GetColorsOnBoard(Board board)
    {
        return ColorsOnBoardToReturn;
    }

    public void RemoveMatchingColorCards(Player player, Board board)
    {
        RemoveMatchingColorCardsCallCount++;
    }

    public void ComputeScores(Player[] players)
    {
        ComputeScoresCallCount++;
    }

    public List<NumberCard> GetStrongestCards(Player player)
    {
        return StrongestCardsToReturn;
    }

    public NumberCard? RemoveStrongestCard(Player player, NumberCard chosen)
    {
        player.RemoveNumberCard(chosen);
        return chosen;
    }
}
