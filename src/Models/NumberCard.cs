using JinxApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Runtime.Serialization;

namespace JinxApp.Models
{
    /// <summary>
    /// Représente une carte numérotée du jeu, caractérisée par une valeur et une couleur.
    /// Hérite de <see cref="Card"/>.
    /// </summary>
    [DataContract(Name = "numberCard")]
    public class NumberCard : Card
    {
        private int value;

        /// <summary>
        /// Valeur numérique de la carte (entre 1 et 6).
        /// </summary>
        [DataMember(Name = "value", Order = 0)]
        public int Value {
            get=>this.value; 
            set
            {
                this.value = value;
                OnPropertyChanged();
            } }

        private CardColor color;

        /// <summary>
        /// Couleur de la carte.
        /// </summary>
        [DataMember(Name = "color", Order = 1)]
        public CardColor Color
        {
            get => color;
            private set => color = value;  // ← setter privé
        }

        public string HexColor => Color switch
        {
            CardColor.RED => "#FF0000",
            CardColor.BLUE => "#0000FF",
            CardColor.GREEN => "#00FF00",
            CardColor.YELLOW => "#FFDE21",
            CardColor.PURPLE => "#800080",
            CardColor.ORANGE => "#FFA500",
            CardColor.PINK => "#FFC0CB",
            CardColor.BLACK => "#000000",
            _ => "#FFFFFF"
        };
        private bool isActive = true;

        /// <summary>
        /// Indique si la carte est active (visible sur le plateau) ou ramassée.
        /// </summary>
        [DataMember(Name = "isActive", Order = 2)]
        public bool IsActive
        {
            get => this.isActive;
            set
            {
                this.isActive = value;
                OnPropertyChanged();
            }
        }

        private NumberCard() : base(0) { }

        /// <summary>
        /// Initialise une carte numérotée avec un identifiant, une valeur et une couleur.
        /// </summary>
        /// <param name="id">Identifiant unique de la carte.</param>
        /// <param name="value">Valeur numérique de la carte.</param>
        /// <param name="color">Couleur de la carte.</param>
        public NumberCard(int id, int value, CardColor color) : base(id)
        {
            this.value = value;
            this.color = color;
        }

        
    }
}