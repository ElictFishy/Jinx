using System.Collections.Generic;
using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Interface définissant le contrat du gestionnaire de cartes chance.
/// </summary>
public interface IChanceCardManager
{
    bool CanPickCard(ChanceCard card, Player player);
    void UseCard(ChanceCard card, Player player, Dice dice);

    bool IsMultiColorActive { get; }
    int MultiColorCount { get; }
    bool IsTakeLowCardActive { get; }
    bool IsTakeHighCardActive { get; }

    bool IsValidLowCard(NumberCard card);
    bool IsValidHighCard(NumberCard card);
    bool IsValidMultiColorSelection(IEnumerable<NumberCard> cards, int diceValue);
    void ApplyMultiColorSelection(IEnumerable<NumberCard> cards, Player player, Board board);

    void ClearActiveEffect();
    void ClearMultiColor();
    void ResetTurnEffects();

    /// <summary>Génère et retourne une carte chance aléatoire parmi tous les types.</summary>
    ChanceCard DrawRandomChanceCard();
}