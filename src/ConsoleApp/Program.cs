using JinxApp.Managers;
using JinxApp.Models;

const int MAXROUNDS = 3;
int joueurs = 0, row = -1, col = -1;
bool? isAI;
string? reponse = "";
string? nom = "";

Console.WriteLine("Bienvenue sur Jinx");

List<Player> players = new();

while (joueurs < 2 || joueurs > 4)
{
    Console.Write("Nombre de joueurs (2 à 4) : ");
    int.TryParse(Console.ReadLine(), out joueurs);
}

Console.WriteLine($"Il y aura {joueurs} joueur(s) !");

for (int i = 0; i < joueurs; i++)
{
    nom = "";
    reponse = "";
    Console.WriteLine($"Le joueur {i + 1} est une IA ? (O/N)");
    while (reponse != "O" && reponse != "N")
        reponse = Console.ReadLine();

    while (string.IsNullOrWhiteSpace(nom))
    {
        Console.WriteLine($"Entrez le nom du joueur {i + 1} : ");
        nom = Console.ReadLine();
    }

    isAI = reponse == "O";
    players.Add(new Player(nom, isAI));
}

Game game = new Game(MAXROUNDS, players);
game.Board.Setup(game.DrawBoardCards());

GameManager gm = new GameManager(
    game,
    new TurnManager(players),
    new VictoryManager(),
    new DiceManager(),
    new ChanceCardManager(),
    new PlacementManager(),
    new AiManager()
);

// =====================
// Abonnement aux events
// =====================
gm.ActivePlayerChanged += (s, e) =>
    Console.WriteLine($"\nC'est au tour de {gm.CurrentPlayer?.Name} !");

gm.OnCardPicked += (s, e) =>
    Console.WriteLine($"{e.Player.Name} a pris la carte {e.Card?.Value}/{e.Card?.Color} !");

gm.BoardChanged += (s, e) =>
    gm.Game.Board.Display();

gm.RoundEnded += (s, e) =>
{
    Console.WriteLine("\n=== Scores ===");
    foreach (Player p in game.Players)
        Console.WriteLine($"{p.Name} : {p.Score} pts");
};

gm.GameFinished += (s, e) =>
    Console.WriteLine("\n=== Partie terminée ! ===");

// =====================
// Boucle de jeu
// =====================
players = players.OrderBy(_ => Random.Shared.Next()).ToList();
gm.CurrentPlayer = players[0];

while (!gm.IsGameOver())
{
    Console.WriteLine(gm.Game);
    gm.Game.Board.Display();

    Console.WriteLine($"C'est au tour de {gm.CurrentPlayer?.Name} !");
    Console.WriteLine($"Deck de {gm.CurrentPlayer?.Name} : {string.Join(", ", gm.CurrentPlayer?.NumberCards.Select(c => $"{c.Value}/{c.Color}") ?? [])}");

    gm.Game.Dice.Value = gm.RollDice();
    Console.WriteLine($"{gm.CurrentPlayer?.Name} lance le dé : {gm.Game.Dice.Value}");

    reponse = "";
    Console.WriteLine("Voulez-vous relancer ? (O/N)");
    while (reponse != "O" && reponse != "N")
        reponse = Console.ReadLine();

    if (reponse == "O")
    {
        gm.Game.Dice.Value = gm.ReRollDice();
        Console.WriteLine($"Nouveau résultat : {gm.Game.Dice.Value}");
    }

    bool roundOver = gm.IsRoundOver();

    if (roundOver)
    {
        Console.WriteLine($"{gm.CurrentPlayer?.Name} ne peut pas jouer, fin de manche !");
    }
    else
    {
        gm.Game.Board.Display();
        Console.WriteLine($"Résultat du dé : {gm.Game.Dice.Value}");

        bool cardPicked = false;
        while (!cardPicked)
        {
            Console.WriteLine("Choisissez une carte :");
            Console.WriteLine("Ligne :");
            if (!int.TryParse(Console.ReadLine(), out row)) { Console.WriteLine("Invalide."); continue; }
            Console.WriteLine("Colonne :");
            if (!int.TryParse(Console.ReadLine(), out col)) { Console.WriteLine("Invalide."); continue; }

            if (row < 0 || row >= Board.SIZE || col < 0 || col >= Board.SIZE)
            { Console.WriteLine("Hors limites (0 à 3)."); continue; }

            if (!gm.CanPickCard(row, col, gm.Game.Board, gm.Game.Dice.Value))
            {
                NumberCard? card = gm.Game.Board.Grid[row, col];
                Console.WriteLine(card == null
                    ? "Cette case est vide !"
                    : $"Carte invalide ! Elle vaut {card.Value}, le dé indique {gm.Game.Dice.Value}.");
            }
            else
            {
                gm.PickCard(row, col);
                cardPicked = true;
            }
        }

        gm.EndTurn();
    }

    if (roundOver)
    {
        Console.WriteLine($"\n=== Fin de la manche {game.CurrentRound} ===");

        foreach (Player p in game.Players)
        {
            IEnumerable<CardColor> boardColors = gm.GetColorsOnBoard();
            List<NumberCard> removed = p.NumberCards
                .Where(c => boardColors.Contains(c.Color))
                .ToList();

            if (removed.Any())
                Console.WriteLine($"{p.Name} perd {removed.Count} carte(s) de couleur encore présente sur le plateau !");
        }

        // Étape 2 — Carte la plus forte du joueur déclencheur
        if (gm.CurrentPlayer != null)
        {
            VictoryManager vm = new VictoryManager(); // ou castez depuis gm si exposé
            List<NumberCard> strongest = vm.GetStrongestCards(gm.CurrentPlayer);

            if (strongest.Count == 1)
            {
                vm.RemoveStrongestCard(gm.CurrentPlayer, strongest[0]);
                Console.WriteLine($"{gm.CurrentPlayer.Name} rend sa carte la plus forte : {strongest[0].Value}/{strongest[0].Color}");
            }
            else if (strongest.Count > 1)
            {
                Console.WriteLine($"\n{gm.CurrentPlayer.Name}, vous avez plusieurs cartes de valeur maximale, choisissez-en une à rendre :");
                for (int i = 0; i < strongest.Count; i++)
                    Console.WriteLine($"  [{i}] {strongest[i].Value}/{strongest[i].Color}");

                int index = -1;
                while (index < 0 || index >= strongest.Count)
                {
                    Console.Write("Votre choix : ");
                    int.TryParse(Console.ReadLine(), out index);
                }

                NumberCard chosen = strongest[index];
                vm.RemoveStrongestCard(gm.CurrentPlayer, chosen);
                Console.WriteLine($"{gm.CurrentPlayer.Name} rend la carte {chosen.Value}/{chosen.Color}.");
            }
        }

        gm.EndRound();
    }
}

Player? finalWinner = gm.Game.HasBestScore(gm.Game.Players);
Console.WriteLine($"\n=== Fin de la partie ! Gagnant : {finalWinner?.Name} avec {finalWinner?.Score} points ! ===");