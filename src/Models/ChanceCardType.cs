using System;
using System.Collections.Generic;
using System.Text;

namespace JinxApp.Models
{
    /// <summary>
    /// Enumération des types d'effets disponibles pour les cartes chance.
    /// </summary>
    public enum ChanceCardType
    {
        /// <summary>Augmente la valeur du dé de 1.</summary>
        INCREASE_DICE,
        /// <summary>Diminue la valeur du dé de 1.</summary>
        DECREASE_DICE,
        /// <summary>Permet de relancer le dé une fois supplémentaire.</summary>
        REROLL_DICE,
        /// <summary>Permet de prendre une carte de n'importe quelle couleur.</summary>
        MULTI_COLOR,
        /// <summary>Permet de prendre la carte de valeur la plus faible du plateau.</summary>
        TAKE_LOW_CARD,
        /// <summary>Permet de prendre la carte de valeur la plus haute du plateau.</summary>
        TAKE_HIGH_CARD
    }
}