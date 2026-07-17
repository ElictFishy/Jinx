using JinxApp.Models;
using JinxApp.Managers;
using Xunit;

namespace Managers.Tests;

public class PlacementManagerTests
{

    /// <summary>Crée un plateau 4x4 entièrement vide.</summary>
    private static Board CreateEmptyBoard()
    {
        Board board = new Board();
        // Ne pas appeler Setup : grille initialisée à null
        return board;
    }

    /// <summary>Crée un plateau avec une seule carte placée en (row, col).</summary>
    private static Board CreateBoardWithCard(int row, int col, int value, CardColor color, out NumberCard placed)
    {
        Board board = new Board();
        placed = new NumberCard(1, value, color);
        board.Grid[row, col] = placed;
        return board;
    }

    /// <summary>Crée un joueur sans cartes.</summary>
    private static Player CreatePlayer() => new Player("TestPlayer", false);


    [Fact]
    public void IsFree_EmptyCell_ReturnsTrue()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateEmptyBoard();

        Assert.True(pm.IsFree(0, 0, board));
    }

    [Fact]
    public void IsFree_OccupiedCell_ReturnsFalse()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED, out _);

        Assert.False(pm.IsFree(0, 0, board));
    }

    /// <summary>Vérifie IsFree sur plusieurs positions vides.</summary>
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    [InlineData(3, 3)]
    [InlineData(1, 2)]
    public void IsFree_VariousEmptyCells_ReturnsTrue(int row, int col)
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateEmptyBoard();

        Assert.True(pm.IsFree(row, col, board));
    }

    /// <summary>Vérifie IsFree sur plusieurs positions occupées.</summary>
    [Theory]
    [InlineData(0, 0, 1, CardColor.RED)]
    [InlineData(2, 3, 4, CardColor.BLUE)]
    [InlineData(3, 3, 6, CardColor.GREEN)]
    public void IsFree_OccupiedCells_ReturnsFalse(int row, int col, int value, CardColor color)
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(row, col, value, color, out _);

        Assert.False(pm.IsFree(row, col, board));
    }


    [Fact]
    public void CanPickCard_NullCell_ReturnsFalse()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateEmptyBoard();

        Assert.False(pm.CanPickCard(0, 0, board, 3));
    }

    [Fact]
    public void CanPickCard_MatchingValue_ReturnsTrue()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(1, 1, 4, CardColor.RED, out _);

        Assert.True(pm.CanPickCard(1, 1, board, 4));
    }

    [Fact]
    public void CanPickCard_WrongValue_ReturnsFalse()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(1, 1, 4, CardColor.RED, out _);

        Assert.False(pm.CanPickCard(1, 1, board, 5));
    }

    /// <summary>Paramétrise valeur du dé vs valeur de la carte.</summary>
    public static TheoryData<int, int, bool> CanPickCardData => new()
    {
        { 1, 1, true },
        { 2, 2, true },
        { 6, 6, true },
        { 3, 4, false },
        { 6, 1, false },
        { 1, 6, false },
    };

    [Theory]
    [MemberData(nameof(CanPickCardData))]
    public void CanPickCard_VariousValues_ReturnsExpected(int cardValue, int diceValue, bool expected)
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, cardValue, CardColor.BLUE, out _);

        Assert.Equal(expected, pm.CanPickCard(0, 0, board, diceValue));
    }



    [Fact]
    public void PickCard_NullCell_ReturnsNull()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateEmptyBoard();
        Player player = CreatePlayer();

        NumberCard? result = pm.PickCard(0, 0, board, player);

        Assert.Null(result);
    }

    [Fact]
    public void PickCard_NullCell_DoesNotAddCardToPlayer()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateEmptyBoard();
        Player player = CreatePlayer();

        pm.PickCard(0, 0, board, player);

        Assert.Empty(player.NumberCards);
    }

    [Fact]
    public void PickCard_ValidCard_ReturnsCard()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED, out NumberCard placed);
        Player player = CreatePlayer();

        NumberCard? result = pm.PickCard(0, 0, board, player);

        Assert.NotNull(result);
        Assert.Equal(placed.Value, result!.Value);
        Assert.Equal(placed.Color, result.Color);
    }

    [Fact]
    public void PickCard_ValidCard_RemovesCardFromBoard()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(2, 2, 5, CardColor.GREEN, out _);
        Player player = CreatePlayer();

        pm.PickCard(2, 2, board, player);

        Assert.True(board.Grid[2, 2] == null || !board.Grid[2, 2]!.IsActive);
    }

    [Fact]
    public void PickCard_ValidCard_AddsActiveCardToPlayer()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED, out NumberCard placed);
        Player player = CreatePlayer();

        pm.PickCard(0, 0, board, player);

        Assert.Contains(player.NumberCards,
            c => c.Value == placed.Value && c.Color == placed.Color && c.IsActive);
    }

    [Fact]
    public void PickCard_ValidCard_PlayerHasExactlyOneCard()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(1, 3, 2, CardColor.BLUE, out _);
        Player player = CreatePlayer();

        pm.PickCard(1, 3, board, player);

        Assert.Single(player.NumberCards);
    }

    /// <summary>Vérifie le pick sur plusieurs positions et couleurs.</summary>
    public static TheoryData<int, int, int, CardColor> PickCardPositionData => new()
    {
        { 0, 0, 1, CardColor.RED },
        { 0, 3, 6, CardColor.BLUE },
        { 3, 0, 3, CardColor.GREEN },
        { 3, 3, 5, CardColor.YELLOW },
        { 2, 1, 4, CardColor.RED },
    };

    [Theory]
    [MemberData(nameof(PickCardPositionData))]
    public void PickCard_VariousPositions_AddsCorrectCardToPlayer(int row, int col, int value, CardColor color)
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(row, col, value, color, out _);
        Player player = CreatePlayer();

        pm.PickCard(row, col, board, player);

        Assert.Contains(player.NumberCards, c => c.Value == value && c.Color == color);
    }


    [Fact]
    public void FindNumberCards_EmptyDeck_ReturnsNull()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED, out _);

        NumberCard? result = pm.FindNumberCards(new List<NumberCard>(), board);

        Assert.Null(result);
    }

    [Fact]
    public void FindNumberCards_EmptyBoard_ReturnsNull()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateEmptyBoard();
        List<NumberCard> deck = new() { new NumberCard(1, 3, CardColor.RED) };

        NumberCard? result = pm.FindNumberCards(deck, board);

        Assert.Null(result);
    }

    [Fact]
    public void FindNumberCards_ColorMatchOnBoard_ReturnsCard()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED, out _);
        NumberCard handCard = new NumberCard(2, 5, CardColor.RED);
        List<NumberCard> deck = new() { handCard };

        NumberCard? result = pm.FindNumberCards(deck, board);

        Assert.NotNull(result);
        Assert.Equal(handCard, result);
    }

    [Fact]
    public void FindNumberCards_NoColorMatch_ReturnsNull()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.RED, out _);
        List<NumberCard> deck = new() { new NumberCard(2, 5, CardColor.BLUE) };

        NumberCard? result = pm.FindNumberCards(deck, board);

        Assert.Null(result);
    }

    [Fact]
    public void FindNumberCards_MultipleCardsInDeck_ReturnsFirstMatch()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.GREEN, out _);
        NumberCard noMatch = new NumberCard(1, 2, CardColor.RED);
        NumberCard match = new NumberCard(2, 4, CardColor.GREEN);
        List<NumberCard> deck = new() { noMatch, match };

        NumberCard? result = pm.FindNumberCards(deck, board);

        Assert.Equal(match, result);
    }

    [Fact]
    public void FindNumberCards_MultipleMatchingColors_ReturnsFirstInDeck()
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, 3, CardColor.BLUE, out _);
        board.Grid[1, 1] = new NumberCard(3, 5, CardColor.RED);
        NumberCard firstBlue = new NumberCard(1, 2, CardColor.BLUE);
        NumberCard red = new NumberCard(2, 4, CardColor.RED);
        List<NumberCard> deck = new() { firstBlue, red };

        NumberCard? result = pm.FindNumberCards(deck, board);

        Assert.Equal(firstBlue, result);
    }

    /// <summary>Paramétrise la couleur de la main et du plateau.</summary>
    [Theory]
    [InlineData(CardColor.RED, CardColor.RED, true)]
    [InlineData(CardColor.BLUE, CardColor.BLUE, true)]
    [InlineData(CardColor.GREEN, CardColor.GREEN, true)]
    [InlineData(CardColor.YELLOW, CardColor.YELLOW, true)]
    [InlineData(CardColor.RED, CardColor.BLUE, false)]
    [InlineData(CardColor.GREEN, CardColor.YELLOW, false)]
    public void FindNumberCards_ColorComparison_ReturnsExpected(
        CardColor handColor, CardColor boardColor, bool shouldFind)
    {
        PlacementManager pm = new PlacementManager();
        Board board = CreateBoardWithCard(0, 0, 3, boardColor, out _);
        List<NumberCard> deck = new() { new NumberCard(1, 5, handColor) };

        NumberCard? result = pm.FindNumberCards(deck, board);

        if (shouldFind)
            Assert.NotNull(result);
        else
            Assert.Null(result);
    }
}
