using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;

namespace JinxApp.Models
{
    /// <summary>
    /// Wrapper stable pour une case du plateau.
    /// Sa référence ne change jamais dans la ObservableCollection,
    /// ce qui empêche le CollectionView MAUI de réorganiser les items.
    /// </summary>
    public class BoardCell : ObservableObject
    {
        private NumberCard? _card;

        public BoardCell(NumberCard? card) { _card = card; }

        public void Update(NumberCard? card)
        {
            _card = card;
            OnPropertyChanged(nameof(IsVisible));
            OnPropertyChanged(nameof(HexColor));
            OnPropertyChanged(nameof(DisplayValue));
        }

        public NumberCard? Card => _card;
        public bool IsVisible => _card != null && _card.IsActive;
        public string HexColor => _card?.HexColor ?? "#00000000";
        public string DisplayValue => _card?.Value.ToString() ?? "";
    }

    /// <summary>
    /// Représente le plateau de jeu composé d'une grille de cartes numérotées.
    /// Le plateau est une grille de <see cref="SIZE"/>x<see cref="SIZE"/> cases.
    /// </summary>
    [DataContract(Name = "board")]
    public class Board : ObservableObject
    {
        /// <summary>
        /// Ligne de séparation utilisée pour l'affichage du plateau dans la console.
        /// </summary>
        private const string TIRETS = "-------------------------";

        /// <summary>
        /// Taille fixe du plateau (nombre de lignes et de colonnes).
        /// </summary>
        public const int SIZE = 4;

        [DataMember(Name = "gridAsList", Order = 0)]
        private List<NumberCard?> gridFlat = new();

        private NumberCard?[,] grid;


        /// <summary>
        /// Grille de cartes du plateau. Une case peut être <c>null</c> si la carte a été prise.
        /// </summary>
        public NumberCard?[,] Grid => grid;


        /// <summary>
        /// Vue à plat du plateau. Chaque BoardCell est un wrapper stable :
        /// sa référence ne change jamais dans la collection, seules ses propriétés
        /// bindables changent — ce qui empêche le CollectionView de réorganiser les items.
        /// </summary>
        private ObservableCollection<BoardCell> _gridAsList = new();
        public ObservableCollection<BoardCell> GridAsList => _gridAsList;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext sc)
        {
            // Reconstruit la grille 2D depuis la liste plate
            grid = new NumberCard?[SIZE, SIZE];
            for (int i = 0; i < gridFlat.Count && i < SIZE * SIZE; i++)
                grid[i / SIZE, i % SIZE] = gridFlat[i];

            // Reconstruit les wrappers BoardCell pour le binding MAUI : le constructeur
            // étant contourné à la désérialisation, _gridAsList serait sinon null.
            _gridAsList = new ObservableCollection<BoardCell>();
            for (int row = 0; row < SIZE; row++)
                for (int col = 0; col < SIZE; col++)
                    _gridAsList.Add(new BoardCell(grid[row, col]));
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext sc)
        {
            // Met à jour la liste plate avant la sérialisation
            gridFlat = new List<NumberCard?>();
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                    gridFlat.Add(grid[r, c]);
        }

        /// <summary>
        /// Initialise un plateau vide.
        /// </summary>
        public Board()
        {
            grid = new NumberCard?[SIZE, SIZE];
        }

        /// <summary>
        /// Remplit le plateau avec les cartes du deck fourni, mélangées aléatoirement.
        /// </summary>
        /// <param name="deck">Liste de cartes numérotées à placer sur le plateau.</param>
        public void Setup(List<NumberCard> deck)
        {
            List<NumberCard> shuffled = deck.OrderBy(_ => Random.Shared.Next()).ToList();
            int index = 0;
            for (int row = 0; row < SIZE; row++)
                for (int col = 0; col < SIZE; col++)
                    grid[row, col] = shuffled[index++];

            if (_gridAsList.Count == 0)
            {
                // Première initialisation : crée les BoardCells
                for (int row = 0; row < SIZE; row++)
                    for (int col = 0; col < SIZE; col++)
                        _gridAsList.Add(new BoardCell(grid[row, col]));
            }
            else
            {
                // Nouvelle manche : met à jour les BoardCells en place (sans recréer la collection)
                int i = 0;
                for (int row = 0; row < SIZE; row++)
                    for (int col = 0; col < SIZE; col++)
                        _gridAsList[i++].Update(grid[row, col]);
            }
        }

        // RefreshGridAsList supprimé : la ObservableCollection se notifie elle-même.

        /// <summary>
        /// Retire et retourne la carte à la position donnée. La case devient <c>null</c>.
        /// </summary>
        /// <param name="row">Ligne de la carte à retirer.</param>
        /// <param name="col">Colonne de la carte à retirer.</param>
        /// <returns>La carte retirée, ou <c>null</c> si la case était vide.</returns>
        public NumberCard? RemoveCard(int row, int col)
        {
            NumberCard? card = grid[row, col];

            if (card != null)
            {
                card.IsActive = false;
                grid[row, col] = null;

                // Met à jour le wrapper en place — le CollectionView ne bouge pas.
                // Garde : _gridAsList n'est peuplée que par Setup(). Si le plateau a été
                // rempli directement (ex. tests), on évite l'accès hors limites.
                int index = row * SIZE + col;
                if (index >= 0 && index < _gridAsList.Count)
                    _gridAsList[index].Update(null);
            }

            return card;
        }


        public (int row, int col)? FindCardPosition(int cardId)
        {
            for (int row = 0; row < SIZE; row++)
                for (int col = 0; col < SIZE; col++)
                    if (grid[row, col]?.GetId() == cardId && grid[row, col]?.IsActive == true)
                        return (row, col);
            return null;
        }


        /// <summary>
        /// Vérifie si toutes les cases du plateau sont vides.
        /// </summary>
        /// <returns><c>true</c> si le plateau est entièrement vide, <c>false</c> sinon.</returns>
        public bool IsEmpty()
        {
            foreach (NumberCard? card in grid)
                if (card != null && card.IsActive) return false;
            return true;
        }

        /// <summary>
        /// Affiche le plateau dans la console avec les couleurs associées à chaque carte.
        /// </summary>
        public void Display()
        {
#if !ANDROID
            Console.Write("     ");
            for (int col = 0; col < SIZE; col++)
                Console.Write($"  {col}    ");
            Console.WriteLine();
            Console.WriteLine("   -------------------------");

            for (int row = 0; row < SIZE; row++)
            {
                Console.Write($" {row} |");
                for (int col = 0; col < SIZE; col++)
                {
                    if (grid[row, col] == null)
                    {
                        Console.Write("  [ ]  |");
                    }
                    else
                    {
                        NumberCard card = grid[row, col]!;
                        Console.ForegroundColor = GetConsoleColor(card.Color);
                        Console.Write($"  {card.Value}/{card.Color.ToString()[0]}  ");
                        Console.ResetColor();
                        Console.Write("|");
                    }
                }
                Console.WriteLine();
                Console.WriteLine("   -------------------------");
            }
#endif
        }

        /// <summary>
        /// Convertit une couleur de carte en couleur console correspondante.
        /// </summary>
        /// <param name="color">La couleur de la carte.</param>
        /// <returns>La couleur console associée.</returns>
        private static ConsoleColor GetConsoleColor(CardColor color)
        {
            return color switch
            {
                CardColor.RED => ConsoleColor.Red,
                CardColor.BLUE => ConsoleColor.Blue,
                CardColor.GREEN => ConsoleColor.Green,
                CardColor.YELLOW => ConsoleColor.Yellow,
                CardColor.PURPLE => ConsoleColor.Magenta,
                CardColor.ORANGE => ConsoleColor.DarkYellow,
                CardColor.PINK => ConsoleColor.DarkMagenta,
                CardColor.BLACK => ConsoleColor.DarkGray,
                _ => ConsoleColor.White
            };
        }

        /// <summary>
        /// Retourne une représentation textuelle du plateau sans couleurs console.
        /// </summary>
        /// <returns>Chaîne représentant la grille du plateau.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("     ");
            for (int col = 0; col < SIZE; col++)
                sb.Append($"  {col}    ");
            sb.AppendLine();
            sb.AppendLine($"   {TIRETS}");

            for (int row = 0; row < SIZE; row++)
            {
                sb.Append($" {row} |");
                for (int col = 0; col < SIZE; col++)
                {
                    if (grid[row, col] == null)
                        sb.Append("  [ ]  |");
                    else
                        sb.Append($"  {grid[row, col]!.Value}/{grid[row, col]!.Color.ToString()[0]}  |");
                }
                sb.AppendLine();
                sb.AppendLine($"   {TIRETS}");
            }
            return sb.ToString();
        }
    }
}