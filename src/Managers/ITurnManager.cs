using System;
using System.Collections.Generic;
using System.Text;
using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Interface définissant le contrat du gestionnaire de tours.
/// </summary>
public interface ITurnManager
{
    /// <summary>
    /// Retourne le joueur suivant dans la rotation.
    /// </summary>
    /// <param name="players">Liste complète des joueurs.</param>
    /// <param name="currentPlayer">Le joueur qui vient de jouer.</param>
    /// <returns>Le prochain joueur à jouer.</returns>
    public Player NextTurn(List<Player> players, Player currentPlayer);

    /// <summary>
    /// Retourne le joueur courant.
    /// </summary>
    public Player CurrentPlayer { get; }
}