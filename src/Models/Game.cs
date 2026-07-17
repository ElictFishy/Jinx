using System.Collections.ObjectModel;
using JinxApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace JinxApp.Models;

/// <summary>
/// Représente l'état global d'une partie de Jinx.
/// </summary>
[DataContract(Name = "game")]
[KnownType(typeof(Tornade))]
[KnownType(typeof(Orage))]
public class Game : ObservableObject
{
    private int currentRound;

    /// <summary>
    /// Numéro de la manche en cours (commence à 1).
    /// </summary>
    [DataMember(Name = "currentRound", Order = 0)]
    public int CurrentRound { get => currentRound; private set => currentRound = value; }


    private int maxRounds;

    /// <summary>
    /// Nombre total de manches de la partie.
    /// </summary>
    [DataMember(Name = "maxRounds", Order = 1)]
    public int MaxRounds { get => maxRounds; private set => maxRounds = value; }


    private Board board;

    /// <summary>
    /// Plateau de jeu de la partie.
    /// </summary>
    [DataMember(Name = "board", Order = 2)]
    public Board Board
    {
        get => board;
        set { board = value; OnPropertyChanged(nameof(Board)); }
    }

    private Dice dice;

    /// <summary>
    /// Dé utilisé pendant la partie.
    /// </summary>
    [DataMember(Name = "dice", Order = 3)]
    public Dice Dice { get => dice; private set => dice = value; }

    private List<Player> players;

    /// <summary>
    /// Liste des joueurs participant à la partie.
    /// </summary>
    [DataMember(Name = "players", Order = 4)]
    public List<Player> Players { get => players; private set => players = value; }

    private Player? currentPlayer;

    /// <summary>
    /// Joueur dont c'est actuellement le tour. Peut être <c>null</c> avant le début.
    /// </summary>
    [DataMember(Name = "currentPlayer", Order = 5)]
    public Player? CurrentPlayer
    {
        get => currentPlayer;
        set
        {
            currentPlayer = value;
            OnPropertyChanged();
        }
    }

    private List<NumberCard>? deck;

    /// <summary>
    /// Pioche persistante de la partie : créée et mélangée une seule fois à la
    /// construction. Chaque manche y puise les cartes du plateau sans remise,
    /// afin qu'une même carte n'apparaisse pas deux fois durant la partie.
    /// </summary>
    [DataMember(Name = "deck", Order = 6)]
    public List<NumberCard>? Deck { get => deck; private set => deck = value; }

    private Game()
    {
        board = new Board();
        dice = new Dice(6);
        players = new List<Player>();
    }


    /// <summary>
    /// Pioche, sans remise, les cartes nécessaires au remplissage du plateau
    /// (16) depuis la pioche persistante. Si la pioche est épuisée ou absente
    /// (ex. ancienne sauvegarde), elle est recréée et mélangée.
    /// </summary>
    public List<NumberCard> DrawBoardCards()
    {
        int needed = Board.SIZE * Board.SIZE;

        List<NumberCard> currentDeck = (deck == null || deck.Count < needed)
            ? CreateDeck().OrderBy(_ => Random.Shared.Next()).ToList()
            : deck;

        List<NumberCard> drawn = currentDeck.Take(needed).ToList();
        currentDeck.RemoveRange(0, needed);
        deck = currentDeck;
        return drawn;
    }

    /// <summary>
    /// Retourne le joueur ayant le score le plus élevé parmi la liste fournie.
    /// </summary>
    /// <param name="players">Liste des joueurs à comparer.</param>
    /// <returns>Le joueur avec le meilleur score, ou <c>null</c> si la liste est vide.</returns>
    public Player? HasBestScore(List<Player> players)
        => players.OrderByDescending(p => p.Score).FirstOrDefault();

    public Game(int maxRounds, List<Player> players)
    {
        this.maxRounds = maxRounds;
        this.players = players;
        this.currentRound = 1;
        this.board = new Board();
        this.dice = new Dice(6);
        this.deck = CreateDeck().OrderBy(_ => Random.Shared.Next()).ToList();
    }

    /// <summary>
    /// Crée le deck de 48 cartes numérotées (8 couleurs × 6 valeurs).
    /// </summary>
    public List<NumberCard> CreateDeck()
    {
        List<NumberCard> deck = new List<NumberCard>();
        int id = 1;
        foreach (CardColor color in Enum.GetValues<CardColor>())
            for (int value = 1; value <= 6; value++)
                deck.Add(new NumberCard(id++, value, color));
        return deck;
    }

    /// <summary>
    /// Crée le deck de cartes chance :
    /// - 4 cartes réutilisables de chaque type (INCREASE, DECREASE, REROLL, MULTI_COLOR)
    /// - 4 cartes à usage unique de chaque type (TAKE_LOW, TAKE_HIGH)
    /// </summary>
    public static List<ChanceCard> CreateChanceDeck()
    {
        var deck = new List<ChanceCard>();
        int id = 1;

        // Cartes réutilisables (4 de chaque)
        for (int i = 0; i < 4; i++) deck.Add(new ChanceCard(id++, ChanceCardType.INCREASE_DICE, false));
        for (int i = 0; i < 4; i++) deck.Add(new ChanceCard(id++, ChanceCardType.DECREASE_DICE, false));
        for (int i = 0; i < 4; i++) deck.Add(new ChanceCard(id++, ChanceCardType.REROLL_DICE, false));
        for (int i = 0; i < 4; i++) deck.Add(new ChanceCard(id++, ChanceCardType.MULTI_COLOR, false));

        // Cartes à usage unique (4 de chaque)
        for (int i = 0; i < 4; i++) deck.Add(new ChanceCard(id++, ChanceCardType.TAKE_LOW_CARD, true));
        for (int i = 0; i < 4; i++) deck.Add(new ChanceCard(id++, ChanceCardType.TAKE_HIGH_CARD, true));

        return deck;
    }

    /// <summary>
    /// Distribue aléatoirement des cartes chance à chaque joueur.
    /// Chaque joueur reçoit : 1 carte réutilisable + 1 carte à usage unique.
    /// </summary>
    public void DistributeChanceCards()
    {
        List<ChanceCard> deck = CreateChanceDeck()
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        // Sépare réutilisables et usage unique
        var reusable = deck.Where(c => !c.IsSingleUse).ToList();
        var singleUse = deck.Where(c => c.IsSingleUse).ToList();

        int reusableIdx = 0;
        int singleUseIdx = 0;

        foreach (Player player in players)
        {
            if (reusableIdx < reusable.Count)
                player.ChanceCards.Add(reusable[reusableIdx++]);

            if (singleUseIdx < singleUse.Count)
                player.ChanceCards.Add(singleUse[singleUseIdx++]);
        }
    }

    public void NextRound() => currentRound++;

    public void RefreshCurrentPlayer()
    {
        OnPropertyChanged(nameof(CurrentPlayer));
        OnPropertyChanged(nameof(CurrentRound));
    }

    public override string ToString()
    {
        string affich = $"Tour {CurrentRound}/{MaxRounds}";
        foreach (Player player in Players)
            affich += $"\n - {player.Name}: {player.Score} points";
        return affich;
    }
}