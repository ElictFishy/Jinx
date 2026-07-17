using JinxApp.Models;
using Xunit;

namespace Models.Tests;

public class ChanceCardTests
{
    [Fact]
    public void ChanceCard_StoresTypeAndSingleUse()
    {
        var card = new ChanceCard(7, ChanceCardType.TAKE_HIGH_CARD, true);

        Assert.Equal(ChanceCardType.TAKE_HIGH_CARD, card.Type);
        Assert.True(card.IsSingleUse);
        Assert.Equal(7, card.GetId());
    }

    [Fact]
    public void ChanceCard_Reusable_HasInfiniteUsageLabel()
    {
        var card = new ChanceCard(1, ChanceCardType.INCREASE_DICE, false);

        Assert.False(card.IsSingleUse);
        Assert.Equal("∞ usage", card.UsageLabel);
    }

    [Fact]
    public void ChanceCard_SingleUse_HasSingleUsageLabel()
    {
        var card = new ChanceCard(1, ChanceCardType.TAKE_LOW_CARD, true);

        Assert.Equal("1× usage", card.UsageLabel);
    }

    [Theory]
    [InlineData(ChanceCardType.INCREASE_DICE)]
    [InlineData(ChanceCardType.DECREASE_DICE)]
    [InlineData(ChanceCardType.REROLL_DICE)]
    [InlineData(ChanceCardType.MULTI_COLOR)]
    [InlineData(ChanceCardType.TAKE_LOW_CARD)]
    [InlineData(ChanceCardType.TAKE_HIGH_CARD)]
    public void ChanceCard_AllTypes_AreStored(ChanceCardType type)
    {
        var card = new ChanceCard(1, type, false);
        Assert.Equal(type, card.Type);
    }
}
