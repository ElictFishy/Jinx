using JinxApp.Models;
using Xunit;

namespace Models.Tests;

public class NumberCardTests
{
    [Fact]
    public void NumberCard_Constructor_SetsIdValueColor()
    {
        NumberCard card = new NumberCard(42, 3, CardColor.RED);

        Assert.Equal(42, card.GetId());
        Assert.Equal(3, card.Value);
        Assert.Equal(CardColor.RED, card.Color);
    }

    [Fact]
    public void NumberCard_IsActive_DefaultIsTrue()
    {
        NumberCard card = new NumberCard(1, 1, CardColor.BLUE);

        Assert.True(card.IsActive);
    }

    [Fact]
    public void NumberCard_IsActive_SetFalse_UpdatesValue()
    {
        NumberCard card = new NumberCard(1, 1, CardColor.BLUE);

        card.IsActive = false;

        Assert.False(card.IsActive);
    }

    [Theory]
    [InlineData(1, CardColor.RED, "#FF0000")]
    [InlineData(2, CardColor.BLUE, "#0000FF")]
    [InlineData(3, CardColor.GREEN, "#00FF00")]
    [InlineData(4, CardColor.YELLOW, "#FFDE21")]
    [InlineData(5, CardColor.PURPLE, "#800080")]
    [InlineData(6, CardColor.ORANGE, "#FFA500")]
    public void NumberCard_HexColor_ReturnsCorrectHex(int id, CardColor color, string expectedHex)
    {
        NumberCard card = new NumberCard(id, 1, color);

        Assert.Equal(expectedHex, card.HexColor);
    }

    [Theory]
    [InlineData(CardColor.PINK, "#FFC0CB")]
    [InlineData(CardColor.BLACK, "#000000")]
    public void NumberCard_HexColor_RemainingColors(CardColor color, string expectedHex)
    {
        NumberCard card = new NumberCard(1, 1, color);

        Assert.Equal(expectedHex, card.HexColor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(6)]
    public void NumberCard_Value_SetValidValue_UpdatesCorrectly(int value)
    {
        NumberCard card = new NumberCard(1, 1, CardColor.BLUE);

        card.Value = value;

        Assert.Equal(value, card.Value);
    }

    public static TheoryData<int, int, CardColor> CardConstructorData => new()
    {
        { 1,  1, CardColor.RED    },
        { 10, 4, CardColor.GREEN  },
        { 99, 6, CardColor.BLACK  },
    };

    [Theory]
    [MemberData(nameof(CardConstructorData))]
    public void NumberCard_Constructor_AllFields_AreCorrect(int id, int value, CardColor color)
    {
        NumberCard card = new NumberCard(id, value, color);

        Assert.Equal(id, card.GetId());
        Assert.Equal(value, card.Value);
        Assert.Equal(color, card.Color);
    }
}