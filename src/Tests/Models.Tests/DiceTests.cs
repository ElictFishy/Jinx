using JinxApp.Models;
using Xunit;

namespace Models.Tests;

public class DiceTests
{
    [Fact]
    public void Dice_DefaultValue_IsInitialValue()
    {
        Dice dice = new Dice(6);

        Assert.Equal(6, dice.Value);
    }

    [Fact]
    public void Dice_SetValue_UpdatesValue()
    {
        Dice dice = new Dice(6);

        dice.Value = 4;

        Assert.Equal(4, dice.Value);
    }


    public static TheoryData<int> ValidDiceValues => new()
    {
        1, 2, 3, 4, 5, 6
    };

    [Theory]
    [MemberData(nameof(ValidDiceValues))]
    public void Dice_ValidValues_DoNotThrow(int v)
    {
        Dice dice = new Dice(6);

        var ex = Record.Exception(() => dice.Value = v);

        Assert.Null(ex);
    }

    [Theory]
    [MemberData(nameof(ValidDiceValues))]
    public void Dice_Constructor_AcceptsValidValues(int v)
    {
        Dice dice = new Dice(v);

        Assert.Equal(v, dice.Value);
    }

    public static TheoryData<int> InvalidDiceValues => new()
    {
        0, -1, -100, 7, 10, 100
    };

    [Theory]
    [MemberData(nameof(InvalidDiceValues))]
    public void Dice_SetInvalidValue_ThrowsArgumentOutOfRangeException(int v)
    {
        Dice dice = new Dice(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => dice.Value = v);
    }

    [Theory]
    [InlineData(3, 5, 5)]
    [InlineData(1, 6, 6)]
    [InlineData(6, 1, 1)]
    public void Dice_SetValue_OverwritesPreviousValue(int initial, int next, int expected)
    {
        Dice dice = new Dice(initial);

        dice.Value = next;

        Assert.Equal(expected, dice.Value);
    }
}