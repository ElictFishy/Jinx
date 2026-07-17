using JinxApp.Models;
using System.Runtime.Serialization;

namespace JinxApp.Models
{
    /// <summary>
    /// Événement aléatoire Tornade : remplace toutes les cartes actives du plateau
    /// par de nouvelles cartes issues d'un nouveau deck mélangé.
    /// </summary>
    [DataContract(Name = "tornade")]
    public static class Tornade
    {
        public static string Name => "Tornade";

        public static void Apply(Game game)
        {
            // Désactive toutes les cartes actuelles du plateau
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                    if (game.Board.Grid[row, col] != null)
                        game.Board.Grid[row, col]!.IsActive = false;

            // Crée un nouveau deck et remet le plateau en place
            // Setup() met à jour les BoardCells automatiquement
            List<NumberCard> newDeck = game.CreateDeck();
            game.Board.Setup(newDeck);
        }
    }
}