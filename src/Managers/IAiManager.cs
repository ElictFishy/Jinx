using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Contrat du gestionnaire d'intelligence artificielle.
/// </summary>
public interface IAiManager
{
    /// <summary>Exécute le tour complet d'un joueur IA (usage interne).</summary>
    void PlayTurn(Player player, Board board, Dice dice);

    /// <summary>
    /// Phase PRE-DÉ : choisit TAKE_HIGH_CARD ou TAKE_LOW_CARD si pertinent.
    /// Retourne null si aucune n'est utile.
    /// </summary>
    ChanceCard? ChoosePreRollChanceCard(Player player, Board board);

    /// <summary>
    /// Phase POST-DÉ : choisit INCREASE_DICE, DECREASE_DICE ou REROLL_DICE si pertinent.
    /// Retourne null si aucune n'est utile.
    /// </summary>
    ChanceCard? ChoosePostRollChanceCard(Player player, Board board, Dice dice);

    /// <summary>Surcharge de compatibilité — délègue vers ChoosePostRollChanceCard.</summary>
    ChanceCard? ChooseChanceCard(Player player, Board board, Dice dice);

    /// <summary>Surcharge sans contexte — retourne la première carte disponible.</summary>
    ChanceCard ChooseChanceCard(Player player);

    /// <summary>
    /// Applique l'effet d'une carte chance et retourne un message décrivant l'action.
    /// </summary>
    string ApplyChanceCard(ChanceCard card, Player player, Board board, Dice dice);

    /// <summary>
    /// Décide si l'IA échange une de ses cartes numérotées contre une carte chance.
    /// Retourne la carte numérotée à céder, ou null si l'IA ne veut pas échanger.
    /// </summary>
    NumberCard? ChooseCardToExchange(Player player);
}