using JinxApp.Models;

namespace JinxApp.Managers;

/// <summary>
/// Implémentation du gestionnaire de dé.
/// Gère le lancer du dé et la mise à jour de sa valeur.
/// </summary>
public class DiceManager : IDiceManager
{
    /// <summary>
    /// Initialise une nouvelle instance du gestionnaire de dé.
    /// </summary>
    public DiceManager()
    {
    }

    /// <summary>
    /// Lance le dé et met à jour sa valeur avec un résultat aléatoire entre 1 et 6.
    /// </summary>
    /// <param name="dice">Le dé à lancer.</param>
    /// <returns>La valeur obtenue après le lancer.</returns>
    public int Roll(Dice dice)
    {
        int result = Random.Shared.Next(1, 7);
        dice.Value = result;
        return result;
    }
}