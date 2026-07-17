using System;
using System.Collections.Generic;
using System.Linq;
using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Implémentation du gestionnaire de cartes chance.
/// Gère l'utilisation et les effets des cartes chance pendant la partie.
/// </summary>
public class ChanceCardManager : IChanceCardManager
{
    /// <summary>
    /// Effet actif en attente d'être consommé par le PlacementManager.
    /// Concerne TAKE_LOW_CARD, TAKE_HIGH_CARD et MULTI_COLOR.
    /// </summary>
    private ChanceCardType? activeEffect;

    /// <summary>
    /// Une carte TAKE_LOW_CARD ou TAKE_HIGH_CARD a été jouée ce tour.
    /// </summary>
    public bool IsTakeLowCardActive => activeEffect == ChanceCardType.TAKE_LOW_CARD;
    public bool IsTakeHighCardActive => activeEffect == ChanceCardType.TAKE_HIGH_CARD;

    /// <summary>
    /// Nombre de cartes MULTI_COLOR possédées par le joueur courant.
    /// 1 carte = somme exacte ; 2 cartes = somme ±1.
    /// </summary>
    public int MultiColorCount { get; private set; }
    public bool IsMultiColorActive => MultiColorCount > 0;

    // -------------------------------------------------------------------------
    // IChanceCardManager
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public bool CanPickCard(ChanceCard card, Player player)
        => player.ChanceCards.Contains(card);

    /// <summary>
    /// Applique l'effet d'une carte chance et la retire de la main si elle est à usage unique.
    /// Pour INCREASE_DICE / DECREASE_DICE / REROLL_DICE, appelez cette méthode autant de fois
    /// que le joueur souhaite utiliser de cartes du même type.
    /// </summary>
    public void UseCard(ChanceCard card, Player player, Dice dice)
    {
        if (!CanPickCard(card, player))
            throw new InvalidOperationException(
                $"Le joueur {player.Name} ne possède pas cette carte chance.");

        ApplyEffect(card, dice);

        if (card.IsSingleUse)
            player.ChanceCards.Remove(card);
    }

    // -------------------------------------------------------------------------
    // Logique interne
    // -------------------------------------------------------------------------

    private void ApplyEffect(ChanceCard card, Dice dice)
    {
        switch (card.Type)
        {
            // --- Cartes réutilisables : agissent directement sur le dé ---

            case ChanceCardType.INCREASE_DICE:
                // +1 par carte, plafonné à 6
                if (dice.Value < 6)
                    dice.Value += 1;
                break;

            case ChanceCardType.DECREASE_DICE:
                // -1 par carte, plancher à 1
                if (dice.Value > 1)
                    dice.Value -= 1;
                break;

            case ChanceCardType.REROLL_DICE:
                // Relance le dé ; le joueur peut appeler UseCard autant de fois
                // qu'il possède de cartes REROLL_DICE
                dice.Value = Random.Shared.Next(1, 7);
                break;

            case ChanceCardType.MULTI_COLOR:
                // Mémorise le nombre de cartes MULTI_COLOR activées.
                // PlacementManager consulte IsMultiColorActive et MultiColorCount
                // pour savoir si la somme de plusieurs cartes est autorisée (±1 si ≥ 2).
                MultiColorCount++;
                break;

            // --- Cartes à usage unique : mémorisées, consommées par PlacementManager ---

            case ChanceCardType.TAKE_LOW_CARD:
                // Autorise le joueur à prendre directement une carte de valeur 1 à 3.
                activeEffect = ChanceCardType.TAKE_LOW_CARD;
                break;

            case ChanceCardType.TAKE_HIGH_CARD:
                // Autorise le joueur à prendre directement une carte de valeur 4 à 6.
                activeEffect = ChanceCardType.TAKE_HIGH_CARD;
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(card.Type), $"Type de carte chance inconnu : {card.Type}");
        }
    }

    // -------------------------------------------------------------------------
    // Helpers pour PlacementManager
    // -------------------------------------------------------------------------

    /// <summary>
    /// Vérifie si une carte numérotée est prenable avec TAKE_LOW_CARD (valeur 1 à 3).
    /// </summary>
    public bool IsValidLowCard(NumberCard card) =>
        IsTakeLowCardActive && card.Value >= 1 && card.Value <= 3;

    /// <summary>
    /// Vérifie si une carte numérotée est prenable avec TAKE_HIGH_CARD (valeur 4 à 6).
    /// </summary>
    public bool IsValidHighCard(NumberCard card) =>
        IsTakeHighCardActive && card.Value >= 4 && card.Value <= 6;

    /// <summary>
    /// Vérifie si un ensemble de cartes de même couleur est valide pour MULTI_COLOR.
    /// La somme doit être égale au résultat du dé.
    /// Si le joueur possède 2 cartes MULTI_COLOR ou plus, la somme peut être ±1.
    /// </summary>
    /// <param name="cards">Cartes sélectionnées (doivent être de même couleur).</param>
    /// <param name="diceValue">Valeur actuelle du dé.</param>
    public bool IsValidMultiColorSelection(IEnumerable<NumberCard> cards, int diceValue)
    {
        if (!IsMultiColorActive) return false;

        var list = cards.ToList();
        if (list.Count == 0) return false;

        // Toutes les cartes doivent être de même couleur
        CardColor color = list[0].Color;
        if (list.Any(c => c.Color != color)) return false;

        int sum = list.Sum(c => c.Value);

        // Tolérance ±1 si le joueur possède 2 cartes MULTI_COLOR ou plus
        int tolerance = MultiColorCount >= 2 ? 1 : 0;
        return Math.Abs(sum - diceValue) <= tolerance;
    }

    /// <summary>
    /// Applique la sélection MULTI_COLOR : retire chaque carte du board,
    /// l'ajoute à la main du joueur, puis réinitialise l'effet.
    /// À appeler après IsValidMultiColorSelection == true.
    /// </summary>
    /// <param name="cards">Cartes sélectionnées (même couleur, somme valide).</param>
    /// <param name="player">Joueur qui récupère les cartes.</param>
    /// <param name="board">Plateau depuis lequel les cartes sont retirées.</param>
    public void ApplyMultiColorSelection(IEnumerable<NumberCard> cards, Player player, Board board)
    {
        foreach (NumberCard card in cards)
        {
            var pos = board.FindCardPosition(card.GetId());
            if (pos == null)
                throw new InvalidOperationException(
                    $"La carte {card.Value}/{card.Color} est introuvable sur le plateau.");

            board.RemoveCard(pos.Value.row, pos.Value.col);
            player.AddNumberCard(card);
        }

        ClearMultiColor();
    }

    /// <summary>
    /// Consomme l'effet actif (TAKE_LOW / TAKE_HIGH) après utilisation.
    /// Doit être appelé par PlacementManager une fois la carte prise.
    /// </summary>
    public void ClearActiveEffect()
    {
        activeEffect = null;
    }

    /// <summary>
    /// Réinitialise l'état MULTI_COLOR en fin de tour.
    /// Doit être appelé par TurnManager après la prise de carte.
    /// </summary>
    public void ClearMultiColor()
    {
        MultiColorCount = 0;
    }

    /// <summary>
    /// Génère une carte chance aléatoire parmi tous les types disponibles.
    /// </summary>
    public ChanceCard DrawRandomChanceCard()
    {
        var allTypes = Enum.GetValues<ChanceCardType>();
        var type = allTypes[Random.Shared.Next(allTypes.Length)];
        bool isSingleUse = type is ChanceCardType.TAKE_LOW_CARD or ChanceCardType.TAKE_HIGH_CARD;
        return new ChanceCard(Random.Shared.Next(1000, 9999), type, isSingleUse);
    }

    /// <summary>
    /// Remet tous les effets à zéro (appelé en début de tour).
    /// </summary>
    public void ResetTurnEffects()
    {
        ClearActiveEffect();
        ClearMultiColor();
    }
}