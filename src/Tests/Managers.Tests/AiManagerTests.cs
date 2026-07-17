using JinxApp.Models;
using JinxApp.Managers;
using Xunit;

namespace Managers.Tests;

public class AiManagerTests
{
    private static Player CreatePlayer(bool ai = true) => new Player("IA", ai);

    /// <summary>Construit un plateau avec uniquement les cartes fournies.</summary>
    private static Board BoardWith(params (int row, int col, int value, CardColor color)[] cards)
    {
        Board board = new Board();
        foreach (var (r, c, v, col) in cards)
            board.Grid[r, c] = new NumberCard(r * Board.SIZE + c + 1, v, col);
        return board;
    }

    // --- ChooseCardToExchange ---

    [Fact]
    public void ChooseCardToExchange_NoChanceCardAndTwoCards_ReturnsLowest()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        p.AddNumberCard(new NumberCard(1, 5, CardColor.RED));
        p.AddNumberCard(new NumberCard(2, 2, CardColor.BLUE));

        NumberCard? result = ai.ChooseCardToExchange(p);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Value);
    }

    [Fact]
    public void ChooseCardToExchange_AlreadyHasChanceCard_ReturnsNull()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        p.AddNumberCard(new NumberCard(1, 5, CardColor.RED));
        p.AddNumberCard(new NumberCard(2, 2, CardColor.BLUE));
        p.ChanceCards.Add(new ChanceCard(1, ChanceCardType.REROLL_DICE, false));

        Assert.Null(ai.ChooseCardToExchange(p));
    }

    [Fact]
    public void ChooseCardToExchange_FewerThanTwoCards_ReturnsNull()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        p.AddNumberCard(new NumberCard(1, 5, CardColor.RED));

        Assert.Null(ai.ChooseCardToExchange(p));
    }

    // --- ChoosePreRollChanceCard ---

    [Fact]
    public void ChoosePreRollChanceCard_HighCardsDominate_ReturnsTakeHigh()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        var takeHigh = new ChanceCard(1, ChanceCardType.TAKE_HIGH_CARD, true);
        p.ChanceCards.Add(takeHigh);
        Board board = BoardWith((0, 0, 5, CardColor.RED), (0, 1, 6, CardColor.BLUE));

        Assert.Equal(takeHigh, ai.ChoosePreRollChanceCard(p, board));
    }

    [Fact]
    public void ChoosePreRollChanceCard_LowCardsDominate_ReturnsTakeLow()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        var takeLow = new ChanceCard(1, ChanceCardType.TAKE_LOW_CARD, true);
        p.ChanceCards.Add(takeLow);
        Board board = BoardWith((0, 0, 1, CardColor.RED), (0, 1, 2, CardColor.BLUE), (0, 2, 3, CardColor.GREEN));

        Assert.Equal(takeLow, ai.ChoosePreRollChanceCard(p, board));
    }

    [Fact]
    public void ChoosePreRollChanceCard_NoRelevantCard_ReturnsNull()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        Board board = BoardWith((0, 0, 5, CardColor.RED));

        Assert.Null(ai.ChoosePreRollChanceCard(p, board));
    }

    // --- ChoosePostRollChanceCard ---

    [Fact]
    public void ChoosePostRollChanceCard_MatchingCardExists_ReturnsNull()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        p.ChanceCards.Add(new ChanceCard(1, ChanceCardType.INCREASE_DICE, false));
        Board board = BoardWith((0, 0, 3, CardColor.RED));

        Assert.Null(ai.ChoosePostRollChanceCard(p, board, new Dice(3)));
    }

    [Fact]
    public void ChoosePostRollChanceCard_NoMatchCanIncrease_ReturnsIncrease()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        var inc = new ChanceCard(1, ChanceCardType.INCREASE_DICE, false);
        p.ChanceCards.Add(inc);
        Board board = BoardWith((0, 0, 5, CardColor.RED));

        Assert.Equal(inc, ai.ChoosePostRollChanceCard(p, board, new Dice(3)));
    }

    [Fact]
    public void ChoosePostRollChanceCard_NoMatchDiceSix_ReturnsDecrease()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        var dec = new ChanceCard(1, ChanceCardType.DECREASE_DICE, false);
        p.ChanceCards.Add(dec);
        Board board = BoardWith((0, 0, 3, CardColor.RED));

        Assert.Equal(dec, ai.ChoosePostRollChanceCard(p, board, new Dice(6)));
    }

    [Fact]
    public void ChoosePostRollChanceCard_OnlyReroll_ReturnsReroll()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        var reroll = new ChanceCard(1, ChanceCardType.REROLL_DICE, false);
        p.ChanceCards.Add(reroll);
        Board board = BoardWith((0, 0, 3, CardColor.RED));

        Assert.Equal(reroll, ai.ChoosePostRollChanceCard(p, board, new Dice(5)));
    }

    // --- ChooseChanceCard ---

    [Fact]
    public void ChooseChanceCard_NoContext_ReturnsFirst()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        var card = new ChanceCard(1, ChanceCardType.REROLL_DICE, false);
        p.ChanceCards.Add(card);

        Assert.Equal(card, ai.ChooseChanceCard(p));
    }

    [Fact]
    public void ChooseChanceCard_WithContext_DelegatesToPostRoll()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        var inc = new ChanceCard(1, ChanceCardType.INCREASE_DICE, false);
        p.ChanceCards.Add(inc);
        Board board = BoardWith((0, 0, 5, CardColor.RED));

        Assert.Equal(inc, ai.ChooseChanceCard(p, board, new Dice(3)));
    }

    // --- ApplyChanceCard ---

    [Fact]
    public void ApplyChanceCard_Increase_IncrementsDice()
    {
        AiManager ai = new();
        Dice dice = new Dice(3);
        string msg = ai.ApplyChanceCard(new ChanceCard(1, ChanceCardType.INCREASE_DICE, false), CreatePlayer(), new Board(), dice);
        Assert.Equal(4, dice.Value);
        Assert.Contains("augment", msg);
    }

    [Fact]
    public void ApplyChanceCard_IncreaseAtMax_StaysSix()
    {
        AiManager ai = new();
        Dice dice = new Dice(6);
        ai.ApplyChanceCard(new ChanceCard(1, ChanceCardType.INCREASE_DICE, false), CreatePlayer(), new Board(), dice);
        Assert.Equal(6, dice.Value);
    }

    [Fact]
    public void ApplyChanceCard_Decrease_DecrementsDice()
    {
        AiManager ai = new();
        Dice dice = new Dice(3);
        ai.ApplyChanceCard(new ChanceCard(1, ChanceCardType.DECREASE_DICE, false), CreatePlayer(), new Board(), dice);
        Assert.Equal(2, dice.Value);
    }

    [Fact]
    public void ApplyChanceCard_DecreaseAtMin_StaysOne()
    {
        AiManager ai = new();
        Dice dice = new Dice(1);
        ai.ApplyChanceCard(new ChanceCard(1, ChanceCardType.DECREASE_DICE, false), CreatePlayer(), new Board(), dice);
        Assert.Equal(1, dice.Value);
    }

    [Fact]
    public void ApplyChanceCard_Reroll_KeepsDiceInRange()
    {
        AiManager ai = new();
        Dice dice = new Dice(3);
        ai.ApplyChanceCard(new ChanceCard(1, ChanceCardType.REROLL_DICE, false), CreatePlayer(), new Board(), dice);
        Assert.InRange(dice.Value, 1, 6);
    }

    [Fact]
    public void ApplyChanceCard_TakeHigh_TakesHighestCard()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        Board board = BoardWith((0, 0, 2, CardColor.RED), (0, 1, 6, CardColor.BLUE), (0, 2, 4, CardColor.GREEN));

        ai.ApplyChanceCard(new ChanceCard(1, ChanceCardType.TAKE_HIGH_CARD, true), p, board, new Dice(3));

        Assert.Contains(p.NumberCards, c => c.Value == 6);
        Assert.Null(board.Grid[0, 1]);
    }

    [Fact]
    public void ApplyChanceCard_TakeLow_TakesLowestCard()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        Board board = BoardWith((0, 0, 2, CardColor.RED), (0, 1, 6, CardColor.BLUE), (0, 2, 4, CardColor.GREEN));

        ai.ApplyChanceCard(new ChanceCard(1, ChanceCardType.TAKE_LOW_CARD, true), p, board, new Dice(3));

        Assert.Contains(p.NumberCards, c => c.Value == 2);
        Assert.Null(board.Grid[0, 0]);
    }

    [Fact]
    public void ApplyChanceCard_TakeHigh_UpdatesScoreLive()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        Board board = BoardWith((0, 0, 6, CardColor.BLUE));

        ai.ApplyChanceCard(new ChanceCard(1, ChanceCardType.TAKE_HIGH_CARD, true), p, board, new Dice(3));

        // Le score suit la main en direct : prendre une carte de valeur 6 le porte à 6.
        Assert.Equal(6, p.Score);
    }

    [Fact]
    public void ApplyChanceCard_MultiColor_ReturnsMessageWithoutChangingDice()
    {
        AiManager ai = new();
        Dice dice = new Dice(3);
        string msg = ai.ApplyChanceCard(new ChanceCard(1, ChanceCardType.MULTI_COLOR, false), CreatePlayer(), new Board(), dice);
        Assert.Equal(3, dice.Value);
        Assert.False(string.IsNullOrEmpty(msg));
    }

    // --- PlayTurn (méthode interne / héritée) ---

    [Fact]
    public void PlayTurn_NoChanceCard_PicksMatchingCard()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        Board board = BoardWith((0, 0, 3, CardColor.RED));

        ai.PlayTurn(p, board, new Dice(3));

        Assert.Single(p.NumberCards);
        Assert.Equal(3, p.NumberCards[0].Value);
    }

    [Fact]
    public void PlayTurn_NoMatchingCard_AddsNothing()
    {
        AiManager ai = new();
        Player p = CreatePlayer();
        Board board = BoardWith((0, 0, 5, CardColor.RED));

        ai.PlayTurn(p, board, new Dice(3));

        Assert.Empty(p.NumberCards);
    }
}
