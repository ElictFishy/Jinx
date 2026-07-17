using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace JinxApp.Models
{
    /// <summary>
    /// Représente une carte chance que le joueur peut utiliser pour obtenir un avantage.
    /// Hérite de <see cref="Card"/>.
    /// </summary>
    [DataContract(Name = "chanceCard")]
    public class ChanceCard : Card
    {
        private ChanceCardType type;

        /// <summary>
        /// Type d'effet de la carte chance.
        /// </summary>
        [DataMember(Name = "type", Order = 0)]
        public ChanceCardType Type { get => type; private set => type = value; }

        private bool isSingleUse;

        /// <summary>
        /// Indique si la carte est à usage unique (consommée après utilisation).
        /// </summary>
        [DataMember(Name = "isSingleUse", Order = 1)]
        public bool IsSingleUse { get => isSingleUse; private set => isSingleUse = value; }

        /// <summary>
        /// Texte lisible indiquant si la carte est à usage unique ou réutilisable.
        /// Utilisé directement en binding XAML sans converter.
        /// </summary>
        public string UsageLabel => isSingleUse ? "1× usage" : "∞ usage";


        private ChanceCard() : base(0)
        { }

        /// <summary>
        /// Initialise une carte chance avec son type et son mode d'utilisation.
        /// </summary>
        /// <param name="id">Identifiant unique de la carte.</param>
        /// <param name="type">Type d'effet de la carte.</param>
        /// <param name="isSingleUse"><c>true</c> si la carte est consommée après usage.</param>
        public ChanceCard(int id, ChanceCardType type, bool isSingleUse) : base(id)
        {
            this.type = type;
            this.isSingleUse = isSingleUse;
        }
    }
}