using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace JinxApp.Models
{

    [DataContract(Name = "orage")]
    /// <summary>
    /// Événement aléatoire Orage : retire (fait disparaître) un nombre aléatoire
    /// de cartes encore actives du plateau.
    /// </summary>
    public class Orage
    {
        public static string Name => "Orage";

        private readonly int _minCards;
        private readonly int _maxCards;

        /// <summary>
        /// Initialise l'Orage avec le nombre min/max de cartes à retirer.
        /// </summary>
        /// <param name="minCards">Nombre minimum de cartes retirées (défaut : 2).</param>
        /// <param name="maxCards">Nombre maximum de cartes retirées (défaut : 5).</param>
        public Orage(int minCards = 2, int maxCards = 5)
        {
            _minCards = minCards;
            _maxCards = maxCards;
        }

        /// <summary>
        /// Retire un nombre aléatoire (entre min et max) de cartes actives du plateau.
        /// </summary>
        /// <returns>Le nombre de cartes effectivement retirées.</returns>
        public int Apply(Game game)
        {
            // Récupère les positions des cartes encore actives sur le plateau
            List<(int row, int col)> activePositions = new();
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                    if (game.Board.Grid[row, col] != null && game.Board.Grid[row, col]!.IsActive)
                        activePositions.Add((row, col));

            if (activePositions.Count == 0) return 0;

            // Tire aléatoirement entre _minCards et _maxCards positions parmi les actives
            int count = Random.Shared.Next(_minCards, Math.Min(_maxCards, activePositions.Count) + 1);
            List<(int row, int col)> targets = activePositions
                .OrderBy(_ => Random.Shared.Next())
                .Take(count)
                .ToList();

            // Retire chaque carte ciblée (la case devient vide, le BoardCell est mis à jour)
            foreach (var (row, col) in targets)
                game.Board.RemoveCard(row, col);

            return targets.Count;
        }
    }
}