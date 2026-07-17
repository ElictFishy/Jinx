using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Interface définissant le contrat du gestionnaire de placement des cartes sur le plateau.
/// </summary>
public interface IPlacementManager
{
    /// <summary>
    /// Vérifie si la case aux coordonnées données est vide.
    /// </summary>
    /// <param name="x">Ligne de la case.</param>
    /// <param name="y">Colonne de la case.</param>
    /// <param name="board">Le plateau de jeu.</param>
    /// <returns><c>true</c> si la case est vide.</returns>
    bool IsFree(int x, int y, Board board);

    /// <summary>
    /// Retire la carte aux coordonnées données et l'ajoute à la main du joueur.
    /// </summary>
    /// <param name="x">Ligne de la carte.</param>
    /// <param name="y">Colonne de la carte.</param>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="player">Le joueur qui prend la carte.</param>
    /// <returns>La carte prise, ou <c>null</c>.</returns>
    NumberCard? PickCard(int x, int y, Board board, Player player);

    /// <summary>
    /// Vérifie si la carte aux coordonnées données peut être prise selon la valeur du dé.
    /// </summary>
    /// <param name="x">Ligne de la carte.</param>
    /// <param name="y">Colonne de la carte.</param>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="diceValue">Valeur actuelle du dé.</param>
    /// <returns><c>true</c> si la carte peut être prise.</returns>
    bool CanPickCard(int x, int y, Board board, int diceValue);

    /// <summary>
    /// Cherche dans un deck une carte dont la couleur est encore présente sur le plateau.
    /// </summary>
    /// <param name="deck">Liste de cartes numérotées à inspecter.</param>
    /// <param name="board">Le plateau de jeu.</param>
    /// <returns>La carte correspondante ou <c>null</c>.</returns>
    NumberCard? FindNumberCards(IList<NumberCard> deck, Board board);
}