using System;
using System.Collections.Generic;
using System.Text;
using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Implémentation du gestionnaire de tours.
/// Gère la rotation des joueurs en fin de tour.
/// </summary>
public class TurnManager : ITurnManager
{
    private readonly List<Player> players;

    /// <summary>
    /// Initialise le gestionnaire de tours avec la liste des joueurs.
    /// </summary>
    /// <param name="players">Liste des joueurs de la partie.</param>
    public TurnManager(List<Player> players)
    {
        this.players = players;
    }

    /// <summary>
    /// Retourne le joueur suivant dans la rotation après le joueur courant.
    /// </summary>
    /// <param name="players">Liste complète des joueurs.</param>
    /// <param name="currentPlayer">Le joueur qui vient de jouer.</param>
    /// <returns>Le prochain joueur à jouer.</returns>
    public Player NextTurn(List<Player> players, Player currentPlayer)
    {
        int index = players.IndexOf(currentPlayer);
        index = (index + 1) % players.Count;
        return players[index];
    }

    /// <summary>
    /// Retourne le dernier joueur de la liste (utilisé comme joueur courant par défaut).
    /// </summary>
    public Player CurrentPlayer
    {
        get
        {
            return players[players.Count - 1];
        }
    }
}