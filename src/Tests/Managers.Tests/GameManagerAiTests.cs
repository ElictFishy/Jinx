using JinxApp.Models;
using JinxApp.Managers;
using Xunit;

namespace Managers.Tests;

public class GameManagerAiTests
{
    private static List<Player> Players(params bool[] ai)
    {
        var list = new List<Player>();
        for (int i = 0; i < ai.Length; i++)
            list.Add(new Player($"P{i}", ai[i]));
        return list;
    }

    private static GameManager Create(List<Player> players)
    {
        Game game = new Game(3, players);
        game.Board.Setup(game.CreateDeck());
        game.CurrentPlayer = players[0];
        return new GameManager(
            game,
            new TurnManager(players),
            new VictoryManager(),
            new DiceManager(),
            new ChanceCardManager(),
            new PlacementManager(),
            new AiManager());
    }

    // --- PlayAiTurn ---

    [Fact]
    public void PlayAiTurn_ReturnsValidDiceValue()
    {
        var players = Players(true, false);
        var gm = Create(players);

        var result = gm.PlayAiTurn(players[0]);

        Assert.InRange(result.DiceValue, 1, 6);
    }

    [Fact]
    public void PlayAiTurn_WithNumberCardsNoChance_ExchangesForChanceCard()
    {
        var players = Players(true, false);
        var gm = Create(players);
        players[0].AddNumberCard(new NumberCard(1, 2, CardColor.RED));
        players[0].AddNumberCard(new NumberCard(2, 5, CardColor.BLUE));

        var result = gm.PlayAiTurn(players[0]);

        Assert.True(result.DrewChanceCard);
        Assert.False(string.IsNullOrEmpty(result.DrawMessage));
    }

    [Fact]
    public void PlayAiTurn_EmptyHand_DoesNotExchange()
    {
        var players = Players(true, false);
        var gm = Create(players);

        var result = gm.PlayAiTurn(players[0]);

        Assert.False(result.DrewChanceCard);
    }

    // --- Modificateur de dé en attente (avant le lancer) ---

    [Fact]
    public void UseChanceCard_IncreaseBeforeRoll_QueuesPositiveDelta()
    {
        var players = Players(false, false);
        var gm = Create(players);
        var card = new ChanceCard(1, ChanceCardType.INCREASE_DICE, false);
        players[0].ChanceCards.Add(card);

        gm.UseChanceCard(card, players[0]);

        Assert.Equal(1, gm.PendingDiceDelta);
    }

    [Fact]
    public void UseChanceCard_DecreaseBeforeRoll_QueuesNegativeDelta()
    {
        var players = Players(false, false);
        var gm = Create(players);
        var card = new ChanceCard(1, ChanceCardType.DECREASE_DICE, false);
        players[0].ChanceCards.Add(card);

        gm.UseChanceCard(card, players[0]);

        Assert.Equal(-1, gm.PendingDiceDelta);
    }

    [Fact]
    public void RollDice_AppliesAndClearsPendingDelta()
    {
        var players = Players(false, false);
        var gm = Create(players);
        var card = new ChanceCard(1, ChanceCardType.INCREASE_DICE, false);
        players[0].ChanceCards.Add(card);
        gm.UseChanceCard(card, players[0]);

        int value = gm.RollDice();

        Assert.InRange(value, 1, 6);
        Assert.Equal(0, gm.PendingDiceDelta);
    }

    [Fact]
    public void EndTurn_ResetsPendingDelta()
    {
        var players = Players(false, false);
        var gm = Create(players);
        var card = new ChanceCard(1, ChanceCardType.DECREASE_DICE, false);
        players[0].ChanceCards.Add(card);
        gm.UseChanceCard(card, players[0]);

        gm.EndTurn();

        Assert.Equal(0, gm.PendingDiceDelta);
    }

    [Fact]
    public void UseChanceCard_IncreaseAfterRoll_ModifiesDiceImmediately()
    {
        var players = Players(false, false);
        var gm = Create(players);
        gm.RollDice();
        gm.Game.Dice.Value = 3;
        var card = new ChanceCard(1, ChanceCardType.INCREASE_DICE, false);
        players[0].ChanceCards.Add(card);

        gm.UseChanceCard(card, players[0]);

        Assert.Equal(4, gm.Game.Dice.Value);
        Assert.Equal(0, gm.PendingDiceDelta);
    }

    // --- DrawChanceCard ---

    [Fact]
    public void DrawChanceCard_ExchangesNumberCardForChanceCard()
    {
        var players = Players(false);
        var gm = Create(players);
        var give = new NumberCard(1, 3, CardColor.RED);
        players[0].AddNumberCard(give);

        ChanceCard drawn = gm.DrawChanceCard(players[0], give);

        Assert.DoesNotContain(give, players[0].NumberCards);
        Assert.Contains(drawn, players[0].ChanceCards);
    }

    // --- Délégations diverses ---

    [Fact]
    public void GetStrongestCards_ReturnsMaxValueCards()
    {
        var players = Players(false);
        var gm = Create(players);
        players[0].AddNumberCard(new NumberCard(1, 2, CardColor.RED));
        players[0].AddNumberCard(new NumberCard(2, 6, CardColor.BLUE));

        var strongest = gm.GetStrongestCards(players[0]);

        Assert.Single(strongest);
        Assert.Equal(6, strongest[0].Value);
    }

    [Fact]
    public void RemoveStrongestCard_RemovesGivenCard()
    {
        var players = Players(false);
        var gm = Create(players);
        var card = new NumberCard(1, 6, CardColor.BLUE);
        players[0].AddNumberCard(card);

        gm.RemoveStrongestCard(players[0], card);

        Assert.DoesNotContain(card, players[0].NumberCards);
    }

    [Fact]
    public void FindNumberCards_ColorPresentOnBoard_ReturnsCard()
    {
        var players = Players(false);
        var gm = Create(players);
        CardColor present = gm.Game.Board.Grid[0, 0]!.Color;
        var hand = new NumberCard(99, 5, present);
        players[0].AddNumberCard(hand);

        var found = gm.FindNumberCards(players[0], gm.Game.Board);

        Assert.Equal(hand, found);
    }

    [Fact]
    public void IsMultiColorActive_AfterUsingMultiColorCard_IsTrue()
    {
        var players = Players(false);
        var gm = Create(players);
        gm.RollDice();
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        players[0].ChanceCards.Add(card);

        gm.UseChanceCard(card, players[0]);

        Assert.True(gm.IsMultiColorActive);
    }

    [Fact]
    public void CanPickCard_WithTakeLowActive_AcceptsLowCard()
    {
        var players = Players(false);
        var gm = Create(players);
        gm.RollDice();
        gm.Game.Board.Grid[0, 0] = new NumberCard(200, 2, CardColor.RED);
        var take = new ChanceCard(1, ChanceCardType.TAKE_LOW_CARD, true);
        players[0].ChanceCards.Add(take);
        gm.UseChanceCard(take, players[0]);

        Assert.True(gm.IsTakeLowCardActive);
        Assert.True(gm.CanPickCard(0, 0, gm.Game.Board, gm.Game.Dice.Value));
    }

    [Fact]
    public void ApplyMultiColorSelection_TransfersSelectedCards()
    {
        var players = Players(false);
        var gm = Create(players);
        gm.RollDice();
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        players[0].ChanceCards.Add(card);
        gm.UseChanceCard(card, players[0]);

        gm.Game.Board.Grid[0, 0] = new NumberCard(300, 2, CardColor.RED);
        gm.Game.Board.Grid[0, 1] = new NumberCard(301, 3, CardColor.RED);
        var sel = new List<NumberCard> { gm.Game.Board.Grid[0, 0]!, gm.Game.Board.Grid[0, 1]! };

        gm.ApplyMultiColorSelection(sel, players[0]);

        Assert.Contains(players[0].NumberCards, c => c.GetId() == 300);
        Assert.Contains(players[0].NumberCards, c => c.GetId() == 301);
    }

    [Fact]
    public void IsValidMultiColorSelection_DelegatesToManager()
    {
        var players = Players(false);
        var gm = Create(players);
        gm.RollDice();
        gm.Game.Dice.Value = 5;
        var card = new ChanceCard(1, ChanceCardType.MULTI_COLOR, false);
        players[0].ChanceCards.Add(card);
        gm.UseChanceCard(card, players[0]);

        var cards = new List<NumberCard> { new NumberCard(1, 2, CardColor.RED), new NumberCard(2, 3, CardColor.RED) };
        Assert.True(gm.IsValidMultiColorSelection(cards));
    }
}
