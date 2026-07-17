using JinxApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JinxApp.Managers;

/// <summary>
/// Implémentation du gestionnaire de victoire.
/// Gère les conditions de fin de manche, de fin de partie, le calcul des scores
/// et la suppression des cartes de mauvaise couleur.
/// </summary>
public class VictoryManager : IVictoryManager
{
    /// <summary>
    /// Vérifie si au moins une carte du plateau correspond à la valeur du dé,
    /// permettant ainsi à un joueur de jouer.
    /// </summary>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="dice">Le dé avec sa valeur courante.</param>
    /// <returns><c>true</c> si un joueur peut encore jouer, <c>false</c> si la manche doit se terminer.</returns>
    public bool CanPlay(Board board, Dice dice)
    {
        foreach (NumberCard? card in board.Grid)
        {
            if (card != null && card.Value == dice.Value)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Vérifie si la partie est terminée (toutes les manches ont été jouées).
    /// </summary>
    /// <param name="game">L'état actuel de la partie.</param>
    /// <returns><c>true</c> si la partie est terminée, <c>false</c> sinon.</returns>
    public bool IsGameOver(Game game)
    {
        if (game.CurrentRound > game.MaxRounds)
            return true;

        return false;
    }

    /// <summary>
    /// Retourne le joueur ayant le score le plus élevé.
    /// </summary>
    /// <param name="players">Liste des joueurs à comparer.</param>
    /// <returns>Le joueur gagnant.</returns>
    public Player GetWinner(List<Player> players)
    {
        return players.OrderByDescending(p => p.Score).First();
    }

    /// <summary>
    /// Retourne l'ensemble des couleurs encore présentes sur le plateau.
    /// </summary>
    /// <param name="board">Le plateau de jeu.</param>
    /// <returns>Enumération des couleurs restantes sur le plateau.</returns>
    public IEnumerable<CardColor> GetColorsOnBoard(Board board)
    {
        HashSet<CardColor> colors = new HashSet<CardColor>();
        foreach (NumberCard? card in board.Grid)
        {
            if (card != null)
                colors.Add(card.Color);
        }
        return colors;
    }

    /// <summary>
    /// Retire de la main du joueur toutes les cartes dont la couleur est encore présente sur le plateau.
    /// Ces cartes ne comptent pas dans le score.
    /// </summary>
    /// <param name="player">Le joueur dont on nettoie la main.</param>
    /// <param name="board">Le plateau de jeu actuel.</param>
    public List<NumberCard> RemoveMatchingColorCards(Player player, Board board)
    {
        IEnumerable<CardColor> boardColors = GetColorsOnBoard(board);
        List<NumberCard> toRemove = player.NumberCards
            .Where(c => boardColors.Contains(c.Color))
            .ToList();

        foreach (NumberCard? card in toRemove)
        {
            player.RemoveNumberCard(card);
        }

        return toRemove;
    }

    /// <summary>
    /// Calcule et ajoute au score de chaque joueur la somme des valeurs de ses cartes en main.
    /// </summary>
    /// <param name="players">Tableau des joueurs dont on calcule les scores.</param>
    public void ComputeScores(Player[] players)
    {
        foreach (Player? player in players)
        {
            player.Score = player.NumberCards.Sum(c => c.Value);
        }
    }

    /// <summary>
    /// Retourne toutes les cartes de valeur maximale du joueur.
    /// </summary>
    /// <param name="players">Tableau des joueurs.</param>
    public List<NumberCard> GetStrongestCards(Player player)
    {
        if (player.NumberCards.Count == 0)
            return new List<NumberCard>();

        int maxValue = player.NumberCards.Max(c => c.Value);
        return player.NumberCards.Where(c => c.Value == maxValue).ToList();
    }

    /// <summary>
    /// Retire la carte de plus grande valeur du joueur ayant déclenché la fin de manche.
    /// S'il a plusieurs cartes de même valeur maximale, retire la première trouvée.
    /// S'il n'a aucune carte, ne rien faire.
    /// </summary>
    /// <param name="player">Le joueur ayant déclenché la fin de manche.</param>
    /// <returns>La carte retirée, ou null si le joueur n'avait aucune carte.</returns>
    /// <param name="chosen">Carte choisis à enlever par l'utilisateur</param>
    public NumberCard? RemoveStrongestCard(Player player, NumberCard chosen)
    {
        player.RemoveNumberCard(chosen);
        return chosen;
    }
}