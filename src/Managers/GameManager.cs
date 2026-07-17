using System.ComponentModel;
using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Gestionnaire principal du jeu, orchestre le déroulement d'une partie.
/// </summary>
public class GameManager : ObservableObject
{
    private readonly Game game;
    public Game Game => game;

    private readonly ITurnManager turnManager;
    private readonly IVictoryManager victoryManager;
    private readonly IDiceManager diceManager;
    private readonly IPlacementManager placementManager;
    private readonly IChanceCardManager chanceCardManager;
    private readonly IAiManager aiManager;

    // =====================
    // Events
    // =====================
    public event EventHandler? GameFinished;
    public event EventHandler? BoardChanged;
    public event EventHandler? ActivePlayerChanged;
    public event EventHandler<CardPickedEventArgs>? OnCardPicked;
    public event EventHandler? RoundEnded;

    /// <summary>
    /// Déclenché quand un événement aléatoire (Orage / Tornade) se produit.
    /// L'argument est un message décrivant l'événement (pour l'affichage).
    /// </summary>
    public event EventHandler<string>? RandomEventTriggered;

    // =====================
    // Événement aléatoire (Orage / Tornade)
    // Un seul des deux se déclenche une fois dans la partie, dans une manche
    // aléatoire, à un tour aléatoire de cette manche.
    // =====================
    private readonly bool eventIsTornade;
    private readonly int eventRound;       // manche où l'événement se déclenche
    private readonly int eventTurnTarget;  // n° de tour (dans la manche) où il se déclenche
    private bool randomEventFired = false;
    private int lastRoundSeen = 0;
    private int turnsInCurrentRound = 0;

    public GameManager(Game game, ITurnManager turnManager, IVictoryManager victoryManager,
        IDiceManager diceManager, IChanceCardManager chanceCardManager,
        IPlacementManager placementManager, IAiManager aiManager)
    {
        this.game = game;
        this.turnManager = turnManager;
        this.diceManager = diceManager;
        this.victoryManager = victoryManager;
        this.placementManager = placementManager;
        this.chanceCardManager = chanceCardManager;
        this.aiManager = aiManager;   // ← était perdu avant

        // Tirage de l'événement aléatoire : quel événement, quelle manche, quel tour.
        eventIsTornade = Random.Shared.Next(2) == 0;
        eventRound = Random.Shared.Next(1, game.MaxRounds + 1);
        eventTurnTarget = Random.Shared.Next(1, 3); // 1er ou 2e tour de la manche
    }

    /// <summary>
    /// Vérifie, au début de chaque tour, s'il faut déclencher l'événement aléatoire.
    /// Se déclenche une seule fois dans la partie, dans la manche tirée au sort et au
    /// tour cible (ou dès que possible si cette manche a été trop courte).
    /// </summary>
    private void MaybeTriggerRandomEvent()
    {
        if (randomEventFired) return;

        int round = game.CurrentRound;
        if (round != lastRoundSeen)
        {
            lastRoundSeen = round;
            turnsInCurrentRound = 0;
        }
        turnsInCurrentRound++;

        bool reached = (round == eventRound && turnsInCurrentRound >= eventTurnTarget)
                       || round > eventRound; // filet de sécurité si la manche cible fut trop courte
        if (!reached) return;

        randomEventFired = true;

        string message;
        if (eventIsTornade)
        {
            Tornade.Apply(game);
            message = "🌪️ Tornade ! Toutes les cartes du plateau ont été remplacées.";
        }
        else
        {
            int removed = new Orage().Apply(game);
            message = $"🌩️ Orage ! {removed} carte(s) ont disparu du plateau.";
        }

        BoardChanged?.Invoke(this, EventArgs.Empty);
        RandomEventTriggered?.Invoke(this, message);
    }

    /// <summary>Indique si une partie est en cours (au moins une manche jouée ou des joueurs avec des cartes).</summary>
    public bool IsGameInProgress()
        => game.CurrentRound > 0 && !victoryManager.IsGameOver(game);

    private bool hasRolled = false;
    public bool HasRolled
    {
        get => hasRolled;
        private set { hasRolled = value; OnPropertyChanged(nameof(HasRolled)); }
    }

    private bool hasRerolled = false;
    public bool HasRerolled
    {
        get => hasRerolled;
        private set { hasRerolled = value; OnPropertyChanged(nameof(HasRerolled)); }
    }

    /// <summary>
    /// Modificateur de dé en attente : somme des cartes INCREASE/DECREASE jouées
    /// AVANT le lancer. Appliqué (et remis à zéro) au moment du lancer.
    /// </summary>
    private int pendingDiceDelta = 0;
    public int PendingDiceDelta => pendingDiceDelta;

    // =====================
    // Dé (joueur humain)
    // =====================

    /// <summary>Lance le dé (1er lancer du tour) et retourne le résultat.</summary>
    public int RollDice()
    {
        MaybeTriggerRandomEvent();
        chanceCardManager.ResetTurnEffects();
        HasRolled = true;
        HasRerolled = false;
        int value = diceManager.Roll(game.Dice);

        // Applique les cartes chance jouées AVANT le lancer (modificateur en attente).
        if (pendingDiceDelta != 0)
        {
            value = Math.Clamp(value + pendingDiceDelta, 1, 6);
            game.Dice.Value = value;
            pendingDiceDelta = 0;
        }
        return value;
    }

    /// <summary>Relance le dé une seule fois par tour (joueur humain).</summary>
    public int ReRollDice()
    {
        if (HasRerolled) throw new InvalidOperationException("Vous avez déjà relancé le dé !");
        HasRerolled = true;
        return diceManager.Roll(game.Dice);
    }

    // =====================
    // Cartes chance
    // =====================

    /// <summary>Expose l'état MULTI_COLOR pour la vue.</summary>
    public bool IsMultiColorActive => chanceCardManager.IsMultiColorActive;
    public bool IsTakeLowCardActive => chanceCardManager.IsTakeLowCardActive;
    public bool IsTakeHighCardActive => chanceCardManager.IsTakeHighCardActive;

    /// <summary>
    /// Utilise une carte chance (joueur humain). Applique l'effet sur le dé si applicable.
    /// Pour MULTI_COLOR / TAKE_LOW / TAKE_HIGH, l'effet est mémorisé et consulté lors du pick.
    /// </summary>
    public void UseChanceCard(ChanceCard card, Player player)
    {
        // Avant le lancer : les cartes qui modifient le dé sont mises en attente
        // et appliquées au moment du lancer (RollDice).
        if (!HasRolled &&
            (card.Type == ChanceCardType.INCREASE_DICE || card.Type == ChanceCardType.DECREASE_DICE))
        {
            if (!chanceCardManager.CanPickCard(card, player))
                throw new InvalidOperationException(
                    $"Le joueur {player.Name} ne possède pas cette carte chance.");

            pendingDiceDelta += card.Type == ChanceCardType.INCREASE_DICE ? 1 : -1;

            if (card.IsSingleUse)
                player.ChanceCards.Remove(card);
            return;
        }

        chanceCardManager.UseCard(card, player, game.Dice);
    }

    /// <summary>Vérifie si une sélection de cartes est valide pour MULTI_COLOR.</summary>
    public bool IsValidMultiColorSelection(IEnumerable<NumberCard> cards)
        => chanceCardManager.IsValidMultiColorSelection(cards, game.Dice.Value);

    /// <summary>Applique la sélection MULTI_COLOR : transfère toutes les cartes du board vers le joueur.</summary>
    public void ApplyMultiColorSelection(IEnumerable<NumberCard> cards, Player player)
    {
        chanceCardManager.ApplyMultiColorSelection(cards, player, game.Board);
        BoardChanged?.Invoke(this, EventArgs.Empty);
    }

    // =====================
    // Placement
    // =====================

    /// <summary>
    /// Vérifie si le joueur peut prendre la carte à la position donnée.
    /// Tient compte des effets TAKE_LOW / TAKE_HIGH / normal.
    /// </summary>
    public bool CanPickCard(int x, int y, Board board, int diceValue)
    {
        NumberCard? card = board.Grid[x, y];
        if (card == null || !card.IsActive) return false;

        if (chanceCardManager.IsTakeLowCardActive)
            return chanceCardManager.IsValidLowCard(card);

        if (chanceCardManager.IsTakeHighCardActive)
            return chanceCardManager.IsValidHighCard(card);

        return placementManager.CanPickCard(x, y, board, diceValue);
    }

    /// <summary>Fait prendre la carte à la position donnée par le joueur courant.</summary>
    public void PickCard(int x, int y)
    {
        Player? current = CurrentPlayer;
        if (current == null) return;

        NumberCard? card = placementManager.PickCard(x, y, game.Board, current);

        if (chanceCardManager.IsTakeLowCardActive || chanceCardManager.IsTakeHighCardActive)
            chanceCardManager.ClearActiveEffect();

        OnCardPicked?.Invoke(this, new CardPickedEventArgs(current, card, x, y));
        BoardChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Cherche dans la main du joueur une carte de même couleur que celle sur le plateau.</summary>
    public NumberCard? FindNumberCards(Player player, Board board)
        => placementManager.FindNumberCards(player.NumberCards, board);

    /// <summary>Retourne les couleurs encore présentes sur le plateau.</summary>
    public IEnumerable<CardColor> GetColorsOnBoard()
        => victoryManager.GetColorsOnBoard(game.Board);

    /// <summary>Expose les cartes les plus fortes du joueur déclencheur pour lui laisser le choix.</summary>
    public List<NumberCard> GetStrongestCards(Player player)
        => victoryManager.GetStrongestCards(player);

    /// <summary>Retire la carte choisie du joueur.</summary>
    public NumberCard? RemoveStrongestCard(Player player, NumberCard chosen)
        => victoryManager.RemoveStrongestCard(player, chosen);

    /// <summary>
    /// Échange : le joueur cède une de ses NumberCards et reçoit une ChanceCard aléatoire.
    /// </summary>
    /// <returns>La ChanceCard obtenue.</returns>
    public ChanceCard DrawChanceCard(Player player, NumberCard cardToGive)
    {
        player.RemoveNumberCard(cardToGive);
        ChanceCard drawn = chanceCardManager.DrawRandomChanceCard();
        player.ChanceCards.Add(drawn);
        return drawn;
    }

    // =====================
    // Tour IA
    // =====================

    /// <summary>
    /// Résultat détaillé d'un tour IA, utilisé par la vue pour animer chaque étape.
    /// </summary>
    public record AiTurnResult(
        // Carte chance jouée AVANT le dé (TAKE_LOW / TAKE_HIGH) — null si aucune
        ChanceCard? PreRollCard,
        // Message décrivant l'effet de la carte pré-dé (pour le bandeau)
        string? PreRollMessage,
        // Carte chance jouée APRÈS le dé (INCREASE / DECREASE / REROLL) — null si aucune
        ChanceCard? PostRollCard,
        // Message décrivant l'effet de la carte post-dé (pour le bandeau)
        string? PostRollMessage,
        // Valeur du dé final (après relance auto éventuelle) — c'est la valeur de la carte prise
        int DiceValue,
        // Valeur brute du premier lancer (avant toute carte chance)
        int RawDiceValue,
        // Valeur du dé juste avant la relance automatique (après carte chance post-dé éventuelle)
        int DiceBeforeReroll,
        // Vrai si TAKE_LOW/TAKE_HIGH a déjà pris une carte (pas de pick supplémentaire)
        bool PickedCardViaChanceCard,
        // Vrai si une carte normale a été trouvée à prendre
        bool PickedCard,
        // Position de la carte à prendre (null si PickedCard == false)
        int? PickedRow,
        int? PickedCol,
        // Vrai si après la relance il n'y avait toujours aucune carte (fin de tour bredouille)
        bool WasRerolled,
        // Vrai si l'IA a échangé une carte numérotée contre une carte chance ce tour
        bool DrewChanceCard,
        // Message décrivant l'échange (pour le bandeau)
        string? DrawMessage
    );

    /// <summary>
    /// Orchestre le tour complet d'un joueur IA :
    ///   1. Lance le dé
    ///   2. Utilise une carte chance si pertinent (AiManager.ChooseChanceCard)
    ///   3. Si aucune carte disponible après la carte chance → relance le dé (une fois)
    ///   4. Sélectionne une carte sur le plateau correspondant au dé final
    /// Retourne un AiTurnResult pour que la vue puisse animer chaque étape.
    /// BoardChanged est levé si une carte est prise via carte chance (TAKE_LOW/TAKE_HIGH).
    /// </summary>
    public AiTurnResult PlayAiTurn(Player player)
    {
        MaybeTriggerRandomEvent();
        chanceCardManager.ResetTurnEffects();
        pendingDiceDelta = 0;
        HasRolled = true;
        HasRerolled = false;

        ChanceCard? chosenCard = null;
        string? chanceMessage = null;
        bool pickedViaChance = false;

        // ── Phase 0 : échanger éventuellement une carte numérotée contre une carte chance ──
        bool drewChanceCard = false;
        string? drawMessage = null;
        NumberCard? cardToExchange = aiManager.ChooseCardToExchange(player);
        if (cardToExchange != null)
        {
            ChanceCard drawn = DrawChanceCard(player, cardToExchange);
            drewChanceCard = true;
            drawMessage = $"🔄 Échange la carte {cardToExchange.Value} contre une carte chance ✦ {drawn.Type}";
        }

        // ── Phase 1 (AVANT le dé) : TAKE_HIGH / TAKE_LOW ────────────────────
        // Ces cartes prennent directement une carte du plateau, indépendamment du dé.
        // L'IA les joue en priorité si le plateau le justifie.
        if (player.ChanceCards.Count > 0)
        {
            chosenCard = aiManager.ChoosePreRollChanceCard(player, game.Board);
            if (chosenCard != null)
            {
                chanceMessage = aiManager.ApplyChanceCard(chosenCard, player, game.Board, game.Dice);
                pickedViaChance = true;
                BoardChanged?.Invoke(this, EventArgs.Empty);
                if (chosenCard.IsSingleUse)
                    player.ChanceCards.Remove(chosenCard);

                return new AiTurnResult(
                    PreRollCard: chosenCard,
                    PreRollMessage: chanceMessage,
                    PostRollCard: null,
                    PostRollMessage: null,
                    DiceValue: game.Dice.Value,
                    RawDiceValue: game.Dice.Value,
                    DiceBeforeReroll: game.Dice.Value,
                    PickedCardViaChanceCard: true,
                    PickedCard: true,
                    PickedRow: null,
                    PickedCol: null,
                    WasRerolled: false,
                    DrewChanceCard: drewChanceCard,
                    DrawMessage: drawMessage
                );
            }
        }

        // ── Phase 2 : lancer le dé ───────────────────────────────────────────
        diceManager.Roll(game.Dice);
        int rawDiceValue = game.Dice.Value;   // valeur brute du premier lancer

        // ── Phase 3 (APRÈS le dé) : INCREASE / DECREASE / REROLL ────────────
        // Ces cartes modifient le dé déjà lancé.
        if (player.ChanceCards.Count > 0)
        {
            chosenCard = aiManager.ChoosePostRollChanceCard(player, game.Board, game.Dice);
            if (chosenCard != null)
            {
                chanceMessage = aiManager.ApplyChanceCard(chosenCard, player, game.Board, game.Dice);
                if (chosenCard.IsSingleUse)
                    player.ChanceCards.Remove(chosenCard);
            }
        }

        // ── Phase 4 : chercher une carte sur le plateau ───────────────────────
        bool wasRerolled = false;
        int diceBeforeReroll = game.Dice.Value;   // valeur testée avant la relance auto
        (int row, int col)? chosen = FindMatchingCard();

        // ── Phase 5 : relance automatique si toujours rien ───────────────────
        if (chosen == null)
        {
            diceManager.Roll(game.Dice);
            HasRerolled = true;
            wasRerolled = true;
            chosen = FindMatchingCard();
        }

        return new AiTurnResult(
            PreRollCard: null,
            PreRollMessage: null,
            PostRollCard: chosenCard,
            PostRollMessage: chanceMessage,
            DiceValue: game.Dice.Value,
            RawDiceValue: rawDiceValue,
            DiceBeforeReroll: diceBeforeReroll,
            PickedCardViaChanceCard: false,
            PickedCard: chosen != null,
            PickedRow: chosen?.row,
            PickedCol: chosen?.col,
            WasRerolled: wasRerolled,
            DrewChanceCard: drewChanceCard,
            DrawMessage: drawMessage
        );
    }

    /// <summary>Retourne la position d'une carte aléatoire correspondant au dé courant, ou null.</summary>
    private (int row, int col)? FindMatchingCard()
    {
        var matches = new List<(int row, int col)>();
        for (int r = 0; r < Board.SIZE; r++)
            for (int c = 0; c < Board.SIZE; c++)
            {
                NumberCard? card = game.Board.Grid[r, c];
                if (card != null && card.IsActive && card.Value == game.Dice.Value)
                    matches.Add((r, c));
            }

        if (matches.Count == 0) return null;
        int n = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, matches.Count);
        return matches[n];
    }

    // =====================
    // Tour / Manche
    // =====================

    /// <summary>Passe au joueur suivant.</summary>
    public void EndTurn()
    {
        if (CurrentPlayer == null) return;
        chanceCardManager.ResetTurnEffects();
        pendingDiceDelta = 0;
        HasRolled = false;
        HasRerolled = false;
        CurrentPlayer = turnManager.NextTurn(game.Players, CurrentPlayer);
        ActivePlayerChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Vérifie si la partie est terminée.</summary>
    public bool IsGameOver()
    {
        bool over = victoryManager.IsGameOver(game);
        if (over) GameFinished?.Invoke(this, EventArgs.Empty);
        return over;
    }

    /// <summary>Vérifie si la manche est terminée.</summary>
    public bool IsRoundOver()
        => !victoryManager.CanPlay(game.Board, game.Dice);

    /// <summary>Cartes perdues par un joueur en fin de manche (couleur encore sur le plateau).</summary>
    public record RoundLoss(Player Player, List<NumberCard> LostCards);

    /// <summary>
    /// Bilan des cartes perdues par chaque joueur lors du dernier <see cref="EndRound"/>,
    /// à cause de la pénalité de couleur. Consulté par la vue pour l'annoncer.
    /// </summary>
    public IReadOnlyList<RoundLoss> LastRoundLosses { get; private set; } = new List<RoundLoss>();

    /// <summary>
    /// Identifiants des cartes que chaque joueur possédait au DÉBUT de la manche
    /// courante. Sert à ne soumettre à la pénalité de couleur que les cartes
    /// GAGNÉES pendant cette manche (les cartes des manches précédentes restent
    /// définitivement acquises).
    /// </summary>
    private readonly Dictionary<Player, HashSet<int>> roundStartCardIds = new();

    /// <summary>Termine la manche en cours et prépare la suivante.</summary>
    public void EndRound()
    {
        // Pénalité de couleur : un joueur perd UNIQUEMENT les cartes gagnées DANS
        // cette manche dont la couleur est encore présente sur le plateau. Les
        // cartes des manches précédentes sont conservées (jamais ré-examinées),
        // si bien que l'annonce ne montre que les pertes de la manche écoulée.
        var boardColors = new HashSet<CardColor>(victoryManager.GetColorsOnBoard(game.Board));
        var losses = new List<RoundLoss>();
        foreach (Player player in game.Players)
        {
            HashSet<int> startIds = roundStartCardIds.TryGetValue(player, out HashSet<int>? ids)
                ? ids : new HashSet<int>();

            List<NumberCard> lost = player.NumberCards
                .Where(c => !startIds.Contains(c.GetId()) && boardColors.Contains(c.Color))
                .ToList();

            foreach (NumberCard card in lost)
                player.RemoveNumberCard(card);

            losses.Add(new RoundLoss(player, lost));
        }
        LastRoundLosses = losses;

        // Score = somme des cartes conservées (le score se synchronise déjà en
        // direct sur la main ; cet appel finalise après la pénalité de couleur).
        // Les cartes ne sont PAS vidées : elles sont conservées toute la partie.
        victoryManager.ComputeScores(game.Players.ToArray());

        game.NextRound();
        game.Board.Setup(game.DrawBoardCards());

        // Mémorise la main de chaque joueur au début de la nouvelle manche (les
        // survivants) pour identifier, à la prochaine fin de manche, les cartes
        // gagnées entre-temps.
        roundStartCardIds.Clear();
        foreach (Player player in game.Players)
            roundStartCardIds[player] = new HashSet<int>(player.NumberCards.Select(c => c.GetId()));

        HasRolled = false;
        HasRerolled = false;
        pendingDiceDelta = 0;
        chanceCardManager.ResetTurnEffects();

        RoundEnded?.Invoke(this, EventArgs.Empty);
        BoardChanged?.Invoke(this, EventArgs.Empty);
    }

    public Player? CurrentPlayer
    {
        get => game.CurrentPlayer;
        set => game.CurrentPlayer = value;
    }

    public Board Board => game.Board;
    public List<Player> Scores => game.Players;
}