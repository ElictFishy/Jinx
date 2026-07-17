using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace JinxApp.Models
{

    [DataContract(Name = "dice")]
    /// <summary>
    /// Représente le dé utilisé pendant la partie.
    /// La valeur du dé doit être comprise entre 1 et 6 inclus.
    /// </summary>
    public class Dice : ObservableObject
    {
        private int value;

        /// <summary>
        /// Valeur actuelle du dé. Doit être comprise entre 1 et 6.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Lancée si la valeur est hors de la plage [1, 6].</exception>
        [DataMember]
        public int Value
        {
            get => value;
            set
            {
                if (value < 1 || value > 6)
                    throw new ArgumentOutOfRangeException(nameof(value), "La valeur n'est pas entre 1 et 6 ");
                this.value = value;
            }
        }
        private Dice()
        { }

        /// <summary>
        /// Initialise le dé avec une valeur de départ.
        /// </summary>
        /// <param name="value">Valeur initiale du dé (entre 1 et 6).</param>
        public Dice(int value)
        {
            this.value = value;
        }
    }
}