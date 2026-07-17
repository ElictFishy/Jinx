using System;
using System.Collections.Generic;
using System.Text;
using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Interface définissant le contrat du gestionnaire de dé.
/// </summary>
public interface IDiceManager
{
    /// <summary>
    /// Lance le dé et retourne la valeur obtenue.
    /// </summary>
    /// <param name="dice">Le dé à lancer.</param>
    /// <returns>La valeur obtenue après le lancer (entre 1 et 6).</returns>
    int Roll(Dice dice);
}