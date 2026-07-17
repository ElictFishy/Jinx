using JinxApp.Models;
using JinxApp.Managers;

namespace JinxApp.Stubs;

/// <summary>
/// Stub du gestionnaire de placement.
/// Toutes les valeurs retournées sont contrôlables depuis le test.
/// </summary>
public class StubPlacementManager : IPlacementManager
{
    public bool IsFreeValue { get; set; } = true;
    public bool CanPickCardValue { get; set; } = true;
    public NumberCard? CardToReturn { get; set; }
    public NumberCard? FindNumberCardsToReturn { get; set; }

    public int IsFreeCallCount { get; private set; } = 0;
    public int CanPickCardCallCount { get; private set; } = 0;
    public int PickCardCallCount { get; private set; } = 0;
    public int FindNumberCardsCallCount { get; private set; } = 0;

    public bool IsFree(int x, int y, Board board)
    {
        IsFreeCallCount++;
        return IsFreeValue;
    }

    public bool CanPickCard(int x, int y, Board board, int diceValue)
    {
        CanPickCardCallCount++;
        return CanPickCardValue;
    }

    public NumberCard? PickCard(int x, int y, Board board, Player player)
    {
        PickCardCallCount++;
        if (CardToReturn != null)
            player.AddNumberCard(CardToReturn);
        return CardToReturn;
    }

    public NumberCard? FindNumberCards(IList<NumberCard> deck, Board board)
    {
        FindNumberCardsCallCount++;
        return FindNumberCardsToReturn;
    }
}
