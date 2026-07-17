using JinxApp.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace JinxApp.Models
{
    /// <summary>
    /// Classe de base abstraite représentant une carte du jeu.
    /// Toutes les cartes (numérotées, chance) héritent de cette classe.
    /// </summary>
    [DataContract(IsReference = true)]
    [KnownType(typeof(NumberCard))]
    [KnownType(typeof(ChanceCard))]
    public abstract class Card: ObservableObject
    {
        [DataMember(Name = "id", Order = 0)]
        private int id;

        protected Card() { }

        /// <summary>
        /// Initialise une carte avec un identifiant unique.
        /// </summary>
        /// <param name="id">Identifiant unique de la carte.</param>
        protected Card(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// Retourne l'identifiant unique de la carte.
        /// </summary>
        /// <returns>L'identifiant de la carte.</returns>
        public int GetId() => id;
    }
}