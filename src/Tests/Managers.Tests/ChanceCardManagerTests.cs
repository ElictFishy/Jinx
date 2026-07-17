using JinxApp.Models;
using JinxApp.Managers;
using Xunit;

namespace Managers.Tests;

public class ChanceCardManagerTests
{
    private static Player PlayerWith(params ChanceCard[] cards)
    {
        Player p = new Player("P", false);
        foreach (var c in cards) p.ChanceCards.Add(c);
        return p;
    }

    [Fact]
    public void UseCard_Increase_IncrementsDiceAndKeepsReusableCard()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.INCREASE_DICE, false);
        var p = PlayerWith(card);
        var dice = new Dice(3);

        mgr.UseCard(card, p, dice);

        Assert.Equal(4, dice.Value);
        Assert.Contains(card, p.ChanceCards);
    }

    [Fact]
    public void UseCard_Decrease_DecrementsDice()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.DECREASE_DICE, false);
        var p = PlayerWith(card);
        var dice = new Dice(3);

        mgr.UseCard(card, p, dice);

        Assert.Equal(2, dice.Value);
    }

    [Fact]
    public void UseCard_Reroll_KeepsDiceInRange()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.REROLL_DICE, false);
        var p = PlayerWith(card);
        var dice = new Dice(3);

        mgr.UseCard(card, p, dice);

        Assert.InRange(dice.Value, 1, 6);
    }

    [Fact]
    public void UseCard_NotOwned_Throws()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.INCREASE_DICE, false);
        var p = new Player("P", false);

        Assert.Throws<InvalidOperationException>(() => mgr.UseCard(card, p, new Dice(3)));
    }

    [Fact]
    public void UseCard_SingleUseTakeLow_RemovesCardAndActivatesEffect()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.TAKE_LOW_CARD, true);
        var p = PlayerWith(card);

        mgr.UseCard(card, p, new Dice(3));

        Assert.True(mgr.IsTakeLowCardActive);
        Assert.DoesNotContain(card, p.ChanceCards);
    }

    [Fact]
    public void UseCard_TakeHigh_ActivatesHighEffect()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.TAKE_HIGH_CARD, true);
        var p = PlayerWith(card);

        mgr.UseCard(card, p, new Dice(3));

        Assert.True(mgr.IsTakeHighCardActive);
    }

    [Fact]
    public void IsValidLowCard_WhenActive_AcceptsLowValuesOnly()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.TAKE_LOW_CARD, true);
        var p = PlayerWith(card);
        mgr.UseCard(card, p, new Dice(3));

        Assert.True(mgr.IsValidLowCard(new NumberCard(1, 2, CardColor.RED)));
        Assert.False(mgr.IsValidLowCard(new NumberCard(2, 5, CardColor.RED)));
    }

    [Fact]
    public void IsValidHighCard_WhenActive_AcceptsHighValuesOnly()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.TAKE_HIGH_CARD, true);
        var p = PlayerWith(card);
        mgr.UseCard(card, p, new Dice(3));

        Assert.True(mgr.IsValidHighCard(new NumberCard(1, 5, CardColor.RED)));
        Assert.False(mgr.IsValidHighCard(new NumberCard(2, 2, CardColor.RED)));
    }

    [Fact]
    public void UseCard_MultiColor_ActivatesMultiColor()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        var p = PlayerWith(card);

        mgr.UseCard(card, p, new Dice(5));

        Assert.True(mgr.IsMultiColorActive);
        Assert.Equal(1, mgr.MultiColorCount);
    }

    [Fact]
    public void IsValidMultiColorSelection_SameColorSumEqualsDice_ReturnsTrue()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        var p = PlayerWith(card);
        mgr.UseCard(card, p, new Dice(5));

        var cards = new List<NumberCard> { new NumberCard(1, 2, CardColor.RED), new NumberCard(2, 3, CardColor.RED) };
        Assert.True(mgr.IsValidMultiColorSelection(cards, 5));
    }

    [Fact]
    public void IsValidMultiColorSelection_DifferentColors_ReturnsFalse()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        var p = PlayerWith(card);
        mgr.UseCard(card, p, new Dice(5));

        var cards = new List<NumberCard> { new NumberCard(1, 2, CardColor.RED), new NumberCard(2, 3, CardColor.BLUE) };
        Assert.False(mgr.IsValidMultiColorSelection(cards, 5));
    }

    [Fact]
    public void IsValidMultiColorSelection_NotActive_ReturnsFalse()
    {
        var mgr = new ChanceCardManager();
        var cards = new List<NumberCard> { new NumberCard(1, 5, CardColor.RED) };
        Assert.False(mgr.IsValidMultiColorSelection(cards, 5));
    }

    [Fact]
    public void IsValidMultiColorSelection_EmptySelection_ReturnsFalse()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        var p = PlayerWith(card);
        mgr.UseCard(card, p, new Dice(5));

        Assert.False(mgr.IsValidMultiColorSelection(new List<NumberCard>(), 5));
    }

    [Fact]
    public void IsValidMultiColorSelection_TwoCards_TolerancePlusOrMinusOne()
    {
        var mgr = new ChanceCardManager();
        var c1 = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        var c2 = new ChanceCard(2, ChanceCardType.MULTI_COLOR, false);
        var p = PlayerWith(c1, c2);
        mgr.UseCard(c1, p, new Dice(5));
        mgr.UseCard(c2, p, new Dice(5));

        Assert.Equal(2, mgr.MultiColorCount);
        // somme = 6, dé = 5, écart 1 toléré car 2 cartes MULTI_COLOR
        var cards = new List<NumberCard> { new NumberCard(1, 2, CardColor.RED), new NumberCard(2, 4, CardColor.RED) };
        Assert.True(mgr.IsValidMultiColorSelection(cards, 5));
    }

    [Fact]
    public void ApplyMultiColorSelection_TransfersCardsAndResets()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        var p = PlayerWith(card);
        mgr.UseCard(card, p, new Dice(5));

        Board board = new Board();
        var c1 = new NumberCard(101, 2, CardColor.RED);
        var c2 = new NumberCard(102, 3, CardColor.RED);
        board.Grid[0, 0] = c1;
        board.Grid[0, 1] = c2;

        mgr.ApplyMultiColorSelection(new List<NumberCard> { c1, c2 }, p, board);

        Assert.Contains(c1, p.NumberCards);
        Assert.Contains(c2, p.NumberCards);
        Assert.Null(board.Grid[0, 0]);
        Assert.Null(board.Grid[0, 1]);
        Assert.False(mgr.IsMultiColorActive);
    }

    [Fact]
    public void ApplyMultiColorSelection_CardNotOnBoard_Throws()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        var p = PlayerWith(card);
        mgr.UseCard(card, p, new Dice(5));

        Board board = new Board();
        var ghost = new NumberCard(999, 2, CardColor.RED);

        Assert.Throws<InvalidOperationException>(
            () => mgr.ApplyMultiColorSelection(new List<NumberCard> { ghost }, p, board));
    }

    [Fact]
    public void DrawRandomChanceCard_ReturnsCardWithValidType()
    {
        var mgr = new ChanceCardManager();
        var card = mgr.DrawRandomChanceCard();
        Assert.Contains(card.Type, Enum.GetValues<ChanceCardType>());
    }

    [Fact]
    public void DrawRandomChanceCard_TakeTypesAreSingleUse()
    {
        var mgr = new ChanceCardManager();
        for (int i = 0; i < 50; i++)
        {
            var card = mgr.DrawRandomChanceCard();
            bool shouldBeSingle = card.Type is ChanceCardType.TAKE_LOW_CARD or ChanceCardType.TAKE_HIGH_CARD;
            Assert.Equal(shouldBeSingle, card.IsSingleUse);
        }
    }

    [Fact]
    public void ResetTurnEffects_ClearsAllEffects()
    {
        var mgr = new ChanceCardManager();
        var multi = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        var take = new ChanceCard(2, ChanceCardType.TAKE_LOW_CARD, true);
        var p = PlayerWith(multi, take);
        mgr.UseCard(multi, p, new Dice(5));
        mgr.UseCard(take, p, new Dice(5));

        mgr.ResetTurnEffects();

        Assert.False(mgr.IsMultiColorActive);
        Assert.False(mgr.IsTakeLowCardActive);
    }

    [Fact]
    public void ClearActiveEffect_DeactivatesTakeEffect()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.TAKE_HIGH_CARD, true);
        var p = PlayerWith(card);
        mgr.UseCard(card, p, new Dice(5));

        mgr.ClearActiveEffect();

        Assert.False(mgr.IsTakeHighCardActive);
    }

    [Fact]
    public void ClearMultiColor_ResetsCount()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        var p = PlayerWith(card);
        mgr.UseCard(card, p, new Dice(5));

        mgr.ClearMultiColor();

        Assert.Equal(0, mgr.MultiColorCount);
        Assert.False(mgr.IsMultiColorActive);
    }

    [Fact]
    public void CanPickCard_OwnedCard_ReturnsTrue()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.REROLL_DICE, false);
        var p = PlayerWith(card);
        Assert.True(mgr.CanPickCard(card, p));
    }

    [Fact]
    public void CanPickCard_NotOwnedCard_ReturnsFalse()
    {
        var mgr = new ChanceCardManager();
        var card = new ChanceCard(1, ChanceCardType.REROLL_DICE, false);
        var p = new Player("P", false);
        Assert.False(mgr.CanPickCard(card, p));
    }
}
