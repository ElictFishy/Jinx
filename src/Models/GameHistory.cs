using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace JinxApp.Models
{
    [DataContract(Name = "gameHistory")]
    /// <summary>
    /// Représente l'historique d'une partie terminée.
    /// Conserve la date de la partie et la liste des joueurs ayant participé.
    /// </summary>
    public class GameHistory : ObservableObject
    {
        [DataMember(Name = "date", Order = 0)]
        private DateTime date;

        [DataMember(Name = "players", Order = 1)]
        private List<Player> players;

        /// <summary>
        /// Constructeur privé pour la désérialisation.
        /// Initialise la liste pour corriger l'avertissement CS8618.
        /// </summary>
        private GameHistory()
        {
            this.date = DateTime.Now;
            this.players = new List<Player>();
        }

        /// <summary>
        /// Initialise un historique de partie avec sa date et ses joueurs.
        /// </summary>
        /// <param name="date">Date à laquelle la partie s'est déroulée.</param>
        /// <param name="players">Liste des joueurs ayant participé à la partie.</param>
        public GameHistory(DateTime date, List<Player> players)
        {
            this.date = date;
            this.players = players;
        }

        /// <summary>
        /// Retourne la date de la partie.
        /// </summary>
        /// <returns>La date de la partie.</returns>
        public DateTime GetDate() => date;

        /// <summary>
        /// Retourne la liste des joueurs ayant participé à la partie.
        /// </summary>
        /// <returns>Liste des joueurs.</returns>
        public List<Player> GetPlayers()
        {
            return players;
        }
    }
}