using JinxApp.Models;
using Xunit;

namespace Models.Tests;

public class BoardTests
{
    private static List<NumberCard> CreateDeck(int count = 16)
    {
        var deck = new List<NumberCard>();
        CardColor[] colors = Enum.GetValues<CardColor>();
        for (int i = 0; i < count; i++)
            deck.Add(new NumberCard(i + 1, (i % 6) + 1, colors[i % colors.Length]));
        return deck;
    }

    // --- Constructeur ---

    [Fact]
    public void Board_Constructor_GridIsNotNull()
    {
        Board board = new Board();

        Assert.NotNull(board.Grid);
    }

    [Fact]
    public void Board_Constructor_GridSizeIs4x4()
    {
        Board board = new Board();

        Assert.Equal(Board.SIZE, board.Grid.GetLength(0));
        Assert.Equal(Board.SIZE, board.Grid.GetLength(1));
    }

    [Fact]
    public void Board_Constructor_AllCellsAreNull()
    {
        Board board = new Board();

        for (int row = 0; row < Board.SIZE; row++)
            for (int col = 0; col < Board.SIZE; col++)
                Assert.Null(board.Grid[row, col]);
    }

    // --- Setup ---

    [Fact]
    public void Board_Setup_AllCellsAreFilled()
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));

        for (int row = 0; row < Board.SIZE; row++)
            for (int col = 0; col < Board.SIZE; col++)
                Assert.NotNull(board.Grid[row, col]);
    }

    [Fact]
    public void Board_Setup_UsesCardsFromDeck()
    {
        Board board = new Board();
        var deck = CreateDeck(16);
        var deckValues = deck.Select(c => c.Value).ToHashSet();

        board.Setup(deck);

        for (int row = 0; row < Board.SIZE; row++)
            for (int col = 0; col < Board.SIZE; col++)
                Assert.Contains(board.Grid[row, col]!.Value, deckValues);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    [InlineData(3, 3)]
    [InlineData(1, 2)]
    public void Board_Setup_SpecificCells_AreNotNull(int row, int col)
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));

        Assert.NotNull(board.Grid[row, col]);
    }

    // --- RemoveCard ---

    [Fact]
    public void Board_RemoveCard_ReturnsCorrectCard()
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));
        NumberCard? expected = board.Grid[0, 0];

        NumberCard? result = board.RemoveCard(0, 0);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Board_RemoveCard_CellBecomesNull()
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));

        board.RemoveCard(1, 2);

        Assert.Null(board.Grid[1, 2]);
    }

    [Fact]
    public void Board_RemoveCard_EmptyCell_ReturnsNull()
    {
        Board board = new Board();

        NumberCard? result = board.RemoveCard(0, 0);

        Assert.Null(result);
    }

    [Fact]
    public void Board_RemoveCard_OtherCellsAreUnchanged()
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));
        NumberCard? neighbor = board.Grid[0, 1];

        board.RemoveCard(0, 0);

        Assert.Equal(neighbor, board.Grid[0, 1]);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    [InlineData(3, 3)]
    public void Board_RemoveCard_VariousCells_CellBecomesNull(int row, int col)
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));

        board.RemoveCard(row, col);

        Assert.Null(board.Grid[row, col]);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    public void Board_RemoveCard_CalledTwice_SecondCallReturnsNull(int row, int col)
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));
        board.RemoveCard(row, col);

        NumberCard? second = board.RemoveCard(row, col);

        Assert.Null(second);
    }

    // --- IsEmpty ---

    [Fact]
    public void Board_IsEmpty_NewBoard_ReturnsTrue()
    {
        Board board = new Board();

        Assert.True(board.IsEmpty());
    }

    [Fact]
    public void Board_IsEmpty_AfterSetup_ReturnsFalse()
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));

        Assert.False(board.IsEmpty());
    }

    [Fact]
    public void Board_IsEmpty_AfterRemovingAllCards_ReturnsTrue()
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));

        for (int row = 0; row < Board.SIZE; row++)
            for (int col = 0; col < Board.SIZE; col++)
                board.RemoveCard(row, col);

        Assert.True(board.IsEmpty());
    }

    [Fact]
    public void Board_IsEmpty_WithOneCardRemaining_ReturnsFalse()
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));

        for (int row = 0; row < Board.SIZE; row++)
            for (int col = 0; col < Board.SIZE; col++)
                if (!(row == 0 && col == 0))
                    board.RemoveCard(row, col);

        Assert.False(board.IsEmpty());
    }

    // --- ToString ---

    [Fact]
    public void Board_ToString_EmptyBoard_ContainsEmptyCells()
    {
        Board board = new Board();

        Assert.Contains("[ ]", board.ToString());
    }

    [Fact]
    public void Board_ToString_AfterSetup_ContainsCardValues()
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));

        Assert.Matches(@"\d/[A-Z]", board.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Board_ToString_ContainsColumnHeader(int col)
    {
        Board board = new Board();

        Assert.Contains(col.ToString(), board.ToString());
    }

    // --- GridAsList ---

    [Fact]
    public void Board_GridAsList_AfterSetup_Has16Elements()
    {
        Board board = new Board();
        board.Setup(CreateDeck(16));

        Assert.Equal(16, board.GridAsList.Count);
    }

    [Fact]
    public void Board_GridAsList_NewBoard_AllElementsAreNull()
    {
        Board board = new Board();

        Assert.All(board.GridAsList, item => Assert.Null(item));
    }
}