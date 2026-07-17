using System;
using System.Collections.Generic;
using System.Text;
using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Interface définissant le contrat du gestionnaire de victoire.
/// Gère les conditions de fin de manche et de partie, ainsi que le calcul des scores.
/// </summary>
public interface IVictoryManager
{
    /// <summary>
    /// Vérifie si un joueur peut encore jouer avec la valeur actuelle du dé.
    /// </summary>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="dice">Le dé avec sa valeur courante.</param>
    /// <returns><c>true</c> si au moins une carte correspond au dé.</returns>
    public bool CanPlay(Board board, Dice dice);

    /// <summary>
    /// Vérifie si la partie est terminée.
    /// </summary>
    /// <param name="game">L'état actuel de la partie.</param>
    /// <returns><c>true</c> si toutes les manches ont été jouées.</returns>
    public bool IsGameOver(Game game);

    /// <summary>
    /// Retourne le joueur ayant le score le plus élevé.
    /// </summary>
    /// <param name="players">Liste des joueurs.</param>
    /// <returns>Le joueur gagnant.</returns>
    public Player GetWinner(List<Player> players);

    /// <summary>
    /// Retourne les couleurs encore présentes sur le plateau.
    /// </summary>
    /// <param name="board">Le plateau de jeu.</param>
    /// <returns>Enumération des couleurs restantes.</returns>
    IEnumerable<CardColor> GetColorsOnBoard(Board board);

    /// <summary>
    /// Retire du joueur toutes les cartes dont la couleur est encore sur le plateau.
    /// </summary>
    /// <param name="player">Le joueur concerné.</param>
    /// <param name="board">Le plateau de jeu.</param>
    /// <returns>La liste des cartes effectivement retirées (vide si aucune).</returns>
    List<NumberCard> RemoveMatchingColorCards(Player player, Board board);

    /// <summary>
    /// Calcule et attribue le score à chaque joueur selon ses cartes en main.
    /// </summary>
    /// <param name="players">Tableau des joueurs.</param>
    void ComputeScores(Player[] players);

    /// <summary>
    /// Retourne toutes les cartes de valeur maximale du joueur.
    /// </summary>
    /// <param name="players">Tableau des joueurs.</param>
    public List<NumberCard> GetStrongestCards(Player player);

    /// <summary>
    /// Retire la carte choisie par le joueur parmi ses cartes les plus fortes.
    /// </summary>
    /// <param name="players">Tableau des joueurs.</param>
    /// <param name="chosen">Carte choisis à enlever par l'utilisateur</param>
    public NumberCard? RemoveStrongestCard(Player player, NumberCard chosen);
}