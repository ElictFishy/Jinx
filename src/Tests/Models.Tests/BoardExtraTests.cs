using JinxApp.Models;
using Xunit;

namespace Models.Tests;

public class BoardExtraTests
{
    private static List<NumberCard> Deck()
    {
        var deck = new List<NumberCard>();
        CardColor[] colors = Enum.GetValues<CardColor>();
        for (int i = 0; i < 16; i++)
            deck.Add(new NumberCard(i + 1, (i % 6) + 1, colors[i % colors.Length]));
        return deck;
    }

    [Fact]
    public void Display_FullBoard_DoesNotThrow()
    {
        var board = new Board();
        board.Setup(Deck());

        board.Display();
        board.RemoveCard(0, 0);
        board.Display(); // case vide affichée

        Assert.False(board.IsEmpty());
    }

    [Fact]
    public void FindCardPosition_ExistingCard_ReturnsPosition()
    {
        var board = new Board();
        board.Setup(Deck());
        NumberCard card = board.Grid[2, 3]!;

        var pos = board.FindCardPosition(card.GetId());

        Assert.NotNull(pos);
        Assert.Equal((2, 3), pos!.Value);
    }

    [Fact]
    public void FindCardPosition_UnknownId_ReturnsNull()
    {
        var board = new Board();
        board.Setup(Deck());

        Assert.Null(board.FindCardPosition(99999));
    }

    [Fact]
    public void FindCardPosition_RemovedCard_ReturnsNull()
    {
        var board = new Board();
        board.Setup(Deck());
        int id = board.Grid[0, 0]!.GetId();
        board.RemoveCard(0, 0);

        Assert.Null(board.FindCardPosition(id));
    }

    [Fact]
    public void Setup_SecondCall_UpdatesExistingCellsInPlace()
    {
        var board = new Board();
        board.Setup(Deck());
        var firstCells = board.GridAsList.ToList();

        board.Setup(Deck()); // nouvelle manche

        // Les wrappers BoardCell ne sont pas recréés (mêmes références)
        Assert.Equal(16, board.GridAsList.Count);
        for (int i = 0; i < 16; i++)
            Assert.Same(firstCells[i], board.GridAsList[i]);
    }

    [Fact]
    public void BoardCell_ReflectsCardState()
    {
        var board = new Board();
        board.Setup(Deck());
        BoardCell cell = board.GridAsList[0];

        Assert.True(cell.IsVisible);
        Assert.NotNull(cell.Card);

        board.RemoveCard(0, 0);

        Assert.False(cell.IsVisible);
        Assert.Equal("#00000000", cell.HexColor);
        Assert.Equal("", cell.DisplayValue);
    }
}
