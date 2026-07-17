using JinxApp.Models;
using JinxApp.Managers;

namespace JinxApp.Stubs;

/// <summary>
/// Stub du gestionnaire de tours.
/// Retourne toujours le joueur suivant dans la liste, contrôlable depuis le test.
/// </summary>
public class StubTurnManager : ITurnManager
{
    public Player? PlayerToReturn { get; set; }
    public int NextTurnCallCount { get; private set; } = 0;

    private Player _currentPlayer;

    public StubTurnManager(Player initialPlayer)
    {
        _currentPlayer = initialPlayer;
    }

    public Player NextTurn(List<Player> players, Player currentPlayer)
    {
        NextTurnCallCount++;
        if (PlayerToReturn != null)
        {
            _currentPlayer = PlayerToReturn;
            return PlayerToReturn;
        }
        int index = players.IndexOf(currentPlayer);
        _currentPlayer = players[(index + 1) % players.Count];
        return _currentPlayer;
    }

    public Player CurrentPlayer => _currentPlayer;
}
