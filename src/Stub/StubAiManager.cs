using JinxApp.Models;
using JinxApp.Managers;

namespace JinxApp.Stubs;

/// <summary>
/// Stub du gestionnaire d'IA.
/// Enregistre les appels sans exécuter de logique réelle.
/// </summary>
public class StubAiManager : IAiManager
{
    public ChanceCard ChanceCardToReturn { get; set; } = new ChanceCard(1, ChanceCardType.TAKE_HIGH_CARD, true);

    public int PlayTurnCallCount { get; private set; } = 0;
    public int ChooseChanceCardCallCount { get; private set; } = 0;

    public void PlayTurn(Player player, Board board, Dice dice)
    {
        PlayTurnCallCount++;
    }

    public ChanceCard ChooseChanceCard(Player player)
    {
        ChooseChanceCardCallCount++;
        return ChanceCardToReturn;
    }
}
