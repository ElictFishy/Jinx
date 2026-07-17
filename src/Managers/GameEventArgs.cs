using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Arguments de l'event déclenché au démarrage de la partie.
/// </summary>
public class GameStartedEventArgs : EventArgs
{
    /// <summary>Liste de tous les joueurs de la partie.</summary>
    public List<Player> Players { get; }

    /// <summary>Le joueur qui commence la partie.</summary>
    public Player FirstPlayer { get; }

    /// <summary>
    /// Initialise les arguments du démarrage de partie.
    /// </summary>
    /// <param name="players">Liste de tous les joueurs.</param>
    /// <param name="firstPlayer">Joueur qui commence.</param>
    public GameStartedEventArgs(List<Player> players, Player firstPlayer)
    {
        Players = players;
        FirstPlayer = firstPlayer;
    }
}

/// <summary>
/// Arguments de l'event déclenché au début du tour d'un joueur.
/// </summary>
public class TurnStartedEventArgs : EventArgs
{
    /// <summary>Le joueur dont c'est le tour.</summary>
    public Player CurrentPlayer { get; }

    /// <summary>
    /// Initialise les arguments du début de tour.
    /// </summary>
    /// <param name="currentPlayer">Le joueur courant.</param>
    public TurnStartedEventArgs(Player currentPlayer)
    {
        CurrentPlayer = currentPlayer;
    }
}

/// <summary>
/// Arguments de l'event déclenché après un lancer de dé.
/// </summary>
public class DiceRolledEventArgs : EventArgs
{
    /// <summary>Valeur obtenue après le lancer.</summary>
    public int Value { get; }

    /// <summary><c>true</c> si c'est une relance, <c>false</c> pour le lancer initial.</summary>
    public bool IsReroll { get; }

    /// <summary>Le joueur qui a lancé le dé.</summary>
    public Player Player { get; }

    /// <summary>
    /// Initialise les arguments du lancer de dé.
    /// </summary>
    /// <param name="value">Valeur obtenue.</param>
    /// <param name="isReroll"><c>true</c> si relance.</param>
    /// <param name="player">Joueur ayant lancé le dé.</param>
    public DiceRolledEventArgs(int value, bool isReroll, Player player)
    {
        Value = value;
        IsReroll = isReroll;
        Player = player;
    }
}

/// <summary>
/// Arguments de l'event déclenché quand un joueur prend une carte.
/// </summary>
public class CardPickedEventArgs : EventArgs
{
    /// <summary>Le joueur qui a pris la carte.</summary>
    public Player Player { get; }

    /// <summary>La carte prise, ou <c>null</c> si la case était vide.</summary>
    public NumberCard? Card { get; }

    /// <summary>Ligne de la carte sur le plateau.</summary>
    public int Row { get; }

    /// <summary>Colonne de la carte sur le plateau.</summary>
    public int Col { get; }

    /// <summary>
    /// Initialise les arguments de la prise de carte.
    /// </summary>
    /// <param name="player">Le joueur qui prend la carte.</param>
    /// <param name="card">La carte prise.</param>
    /// <param name="row">Ligne de la carte.</param>
    /// <param name="col">Colonne de la carte.</param>
    public CardPickedEventArgs(Player player, NumberCard? card, int row, int col)
    {
        Player = player;
        Card = card;
        Row = row;
        Col = col;
    }
}

/// <summary>
/// Arguments de l'event déclenché à la fin du tour d'un joueur.
/// </summary>
public class TurnEndedEventArgs : EventArgs
{
    /// <summary>Le joueur qui vient de terminer son tour.</summary>
    public Player PreviousPlayer { get; }

    /// <summary>Le joueur qui va jouer ensuite.</summary>
    public Player NextPlayer { get; }

    /// <summary>
    /// Initialise les arguments de fin de tour.
    /// </summary>
    /// <param name="previousPlayer">Joueur qui vient de jouer.</param>
    /// <param name="nextPlayer">Prochain joueur.</param>
    public TurnEndedEventArgs(Player previousPlayer, Player nextPlayer)
    {
        PreviousPlayer = previousPlayer;
        NextPlayer = nextPlayer;
    }
}

/// <summary>
/// Arguments de l'event déclenché quand aucun joueur ne peut jouer (fin de manche déclenchée).
/// </summary>
public class RoundOverEventArgs : EventArgs
{
    /// <summary>Le joueur courant au moment de la fin de manche.</summary>
    public Player CurrentPlayer { get; }

    /// <summary>Numéro de la manche qui se termine.</summary>
    public int Round { get; }

    /// <summary>
    /// Initialise les arguments de fin de manche.
    /// </summary>
    /// <param name="currentPlayer">Joueur courant.</param>
    /// <param name="round">Numéro de la manche.</param>
    public RoundOverEventArgs(Player currentPlayer, int round)
    {
        CurrentPlayer = currentPlayer;
        Round = round;
    }
}

/// <summary>
/// Arguments de l'event déclenché après le calcul des scores et le passage à la manche suivante.
/// </summary>
public class RoundEndedEventArgs : EventArgs
{
    /// <summary>Liste des joueurs avec leurs scores mis à jour.</summary>
    public List<Player> Players { get; }

    /// <summary>Numéro de la nouvelle manche.</summary>
    public int NewRound { get; }

    /// <summary>
    /// Initialise les arguments de fin de manche.
    /// </summary>
    /// <param name="players">Liste des joueurs.</param>
    /// <param name="newRound">Numéro de la nouvelle manche.</param>
    public RoundEndedEventArgs(List<Player> players, int newRound)
    {
        Players = players;
        NewRound = newRound;
    }
}

/// <summary>
/// Arguments de l'event déclenché à la fin de la partie.
/// </summary>
public class GameOverEventArgs : EventArgs
{
    /// <summary>Liste finale de tous les joueurs avec leurs scores.</summary>
    public List<Player> Players { get; }

    /// <summary>Le gagnant de la partie, ou <c>null</c> en cas d'égalité parfaite.</summary>
    public Player? Winner { get; }

    /// <summary>
    /// Initialise les arguments de fin de partie.
    /// </summary>
    /// <param name="players">Liste des joueurs.</param>
    /// <param name="winner">Le gagnant.</param>
    public GameOverEventArgs(List<Player> players, Player? winner)
    {
        Players = players;
        Winner = winner;
    }
}