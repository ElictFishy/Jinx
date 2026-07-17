using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Implémentation du gestionnaire de placement des cartes.
/// Gère la validation et la récupération des cartes sur le plateau.
/// </summary>
public class PlacementManager : IPlacementManager
{
    /// <summary>
    /// Initialise une nouvelle instance du gestionnaire de placement.
    /// </summary>
    public PlacementManager() { }

    /// <summary>
    /// Vérifie si la case aux coordonnées données est vide.
    /// </summary>
    /// <param name="x">Ligne de la case.</param>
    /// <param name="y">Colonne de la case.</param>
    /// <param name="board">Le plateau de jeu.</param>
    /// <returns><c>true</c> si la case est vide, <c>false</c> sinon.</returns>
    public bool IsFree(int x, int y, Board board)
    {
        return board.Grid[x, y] == null;
    }

    /// <summary>
    /// Retire la carte aux coordonnées données du plateau et l'ajoute à la main du joueur.
    /// </summary>
    /// <param name="x">Ligne de la carte.</param>
    /// <param name="y">Colonne de la carte.</param>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="player">Le joueur qui prend la carte.</param>
    /// <returns>La carte prise, ou <c>null</c> si la case était vide.</returns>
    public NumberCard? PickCard(int x, int y, Board board, Player player)
    {
        NumberCard? card = board.RemoveCard(x, y); // IsActive = false sur le plateau
        if (card != null)
        {
            // On crée une nouvelle carte avec les mêmes valeurs pour la main du joueur
            NumberCard copy = new NumberCard(card.GetId(), card.Value, card.Color);
            // copy.IsActive = true par défaut dans le constructeur
            player.AddNumberCard(copy);
        }
        return card;
    }

    /// <summary>
    /// Vérifie si le joueur peut prendre la carte aux coordonnées données selon la valeur du dé.
    /// </summary>
    /// <param name="x">Ligne de la carte.</param>
    /// <param name="y">Colonne de la carte.</param>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="diceValue">La valeur actuelle du dé.</param>
    /// <returns><c>true</c> si la carte existe et sa valeur correspond au dé, <c>false</c> sinon.</returns>
    public bool CanPickCard(int x, int y, Board board, int diceValue)
    {
        NumberCard? card = board.Grid[x, y];
        return card != null && card.Value == diceValue;
    }

    /// <summary>
    /// Cherche dans le deck du joueur une carte dont la couleur est encore présente sur le plateau.
    /// </summary>
    /// <param name="deck">La main du joueur (liste de cartes numérotées).</param>
    /// <param name="board">Le plateau de jeu actuel.</param>
    /// <returns>La première carte correspondante, ou <c>null</c> si aucune ne correspond.</returns>
    public NumberCard? FindNumberCards(IList<NumberCard> deck, Board board)
    {
        foreach (NumberCard c in deck)
        {
            for (int i = 0; i < Board.SIZE; i++)
            {
                for (int j = 0; j < Board.SIZE; j++)
                {
                    NumberCard? gridCard = board.Grid[i, j];
                    if (gridCard != null && gridCard.Color == c.Color)
                        return c;
                }
            }
        }
        return null;
    }
}