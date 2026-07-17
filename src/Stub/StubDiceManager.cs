using JinxApp.Models;
using JinxApp.Managers;

namespace JinxApp.Stubs;

/// <summary>
/// Stub du gestionnaire de dé.
/// Retourne une valeur fixe contrôlée par le test.
/// </summary>
public class StubDiceManager : IDiceManager
{
    public int ValueToReturn { get; set; } = 1;
    public int RollCallCount { get; private set; } = 0;

    public int Roll(Dice dice)
    {
        RollCallCount++;
        dice.Value = ValueToReturn;
        return ValueToReturn;
    }
}
