using JinxApp.Models;
using JinxApp.Managers;
using Xunit;

namespace Managers.Tests;

public class DiceManagerTests
{
    [Fact]
    public void Roll_ReturnsValueBetween1And6()
    {
        // Arrange
        DiceManager dm = new DiceManager();
        Dice dice = new Dice(6);
         
        //Act
        int result = dm.Roll(dice);

        //Assert
        Assert.InRange(result, 1, 6);
    }

    [Fact]
    public void Roll_UpdatesDiceValue()
    {
        // Arrange
        DiceManager dm = new DiceManager();
        Dice dice = new Dice(6);

        //Act
        dm.Roll(dice);

        // Assert
        Assert.InRange(dice.Value, 1, 6);
    }
}