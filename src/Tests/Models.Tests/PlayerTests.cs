using JinxApp.Models;
using Xunit;

namespace Models.Tests;

public class PlayerTests
{
    // --- Constructeur ---

    [Fact]
    public void Player_DefaultValues_AreCorrect()
    {
        Player player = new Player("joueur", false);

        Assert.Equal("joueur", player.Name);
        Assert.False(player.IsAi);
        Assert.Equal(0, player.Score);
        Assert.NotNull(player.NumberCards);
        Assert.NotNull(player.ChanceCards);
        Assert.Empty(player.NumberCards);
        Assert.Empty(player.ChanceCards);
    }

    [Fact]
    public void Player_NullName_DefaultsToJoueur()
    {
        Player player = new Player(null!, false);

        Assert.Equal("Joueur", player.Name);
    }

    [Fact]
    public void Player_NullIsAi_DefaultsToFalse()
    {
        Player player = new Player("joueur", null);

        Assert.False(player.IsAi);
    }

    [Fact]
    public void Player_IsAiTrue_SetsCorrectly()
    {
        Player player = new Player("bot", true);

        Assert.True(player.IsAi);
    }

    [Theory]
    [InlineData("Alice", false)]
    [InlineData("Bob", true)]
    [InlineData("", false)]
    public void Player_Constructor_SetsNameAndIsAi(string name, bool isAi)
    {
        Player player = new Player(name, isAi);

        Assert.Equal(name, player.Name);
        Assert.Equal(isAi, player.IsAi);
    }

    // --- Score ---

    [Fact]
    public void Player_Score_SetZero_SetsCorrectly()
    {
        Player player = new Player("joueur", false);

        player.Score = 0;

        Assert.Equal(0, player.Score);
    }

    [Fact]
    public void Player_Score_SetNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        Player player = new Player("joueur", false);

        Assert.Throws<ArgumentOutOfRangeException>(() => player.Score = -1);
    }

    public static TheoryData<int> ValidScores => new() { 0, 1, 10, 100, 9999 };

    [Theory]
    [MemberData(nameof(ValidScores))]
    public void Player_Score_ValidValues_SetCorrectly(int score)
    {
        Player player = new Player("joueur", false);

        player.Score = score;

        Assert.Equal(score, player.Score);
    }

    public static TheoryData<int> InvalidScores => new() { -1, -10, -999 };

    [Theory]
    [MemberData(nameof(InvalidScores))]
    public void Player_Score_NegativeValues_ThrowArgumentOutOfRangeException(int score)
    {
        Player player = new Player("joueur", false);

        Assert.Throws<ArgumentOutOfRangeException>(() => player.Score = score);
    }

    // --- AddNumberCard ---

    [Fact]
    public void Player_AddNumberCard_CardIsInList()
    {
        Player player = new Player("joueur", false);
        NumberCard card = new NumberCard(1, 1, CardColor.BLUE);

        player.AddNumberCard(card);

        Assert.Contains(card, player.NumberCards);
    }

    [Fact]
    public void Player_AddNumberCard_MultipleCards_CountIsCorrect()
    {
        Player player = new Player("joueur", false);

        player.AddNumberCard(new NumberCard(1, 1, CardColor.BLUE));
        player.AddNumberCard(new NumberCard(2, 2, CardColor.RED));

        Assert.Equal(2, player.NumberCards.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Player_AddNumberCard_NCards_CountIsCorrect(int n)
    {
        Player player = new Player("joueur", false);

        for (int i = 0; i < n; i++)
            player.AddNumberCard(new NumberCard(i, i + 1, CardColor.BLUE));

        Assert.Equal(n, player.NumberCards.Count);
    }

    // --- RemoveNumberCard ---

    [Fact]
    public void Player_RemoveNumberCard_CardIsRemovedFromList()
    {
        Player player = new Player("joueur", false);
        NumberCard card = new NumberCard(1, 1, CardColor.BLUE);
        player.AddNumberCard(card);

        player.RemoveNumberCard(card);

        Assert.DoesNotContain(card, player.NumberCards);
    }

    [Fact]
    public void Player_RemoveNumberCard_ListIsEmptyAfterRemoval()
    {
        Player player = new Player("joueur", false);
        NumberCard card = new NumberCard(1, 1, CardColor.BLUE);
        player.AddNumberCard(card);

        player.RemoveNumberCard(card);

        Assert.Empty(player.NumberCards);
    }

    [Fact]
    public void Player_RemoveNumberCard_CardNotInList_DoesNotThrow()
    {
        Player player = new Player("joueur", false);
        NumberCard card = new NumberCard(1, 1, CardColor.BLUE);

        var exception = Record.Exception(() => player.RemoveNumberCard(card));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(CardColor.RED)]
    [InlineData(CardColor.BLUE)]
    [InlineData(CardColor.GREEN)]
    public void Player_RemoveNumberCard_ByColor_RemovesCorrectCard(CardColor color)
    {
        Player player = new Player("joueur", false);
        NumberCard card = new NumberCard(1, 1, color);
        player.AddNumberCard(card);

        player.RemoveNumberCard(card);

        Assert.DoesNotContain(card, player.NumberCards);
    }
}